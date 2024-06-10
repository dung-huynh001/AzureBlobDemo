using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace AzureBlobDemo.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private string GetUserName(HttpContext? context)
        {
            if (context != null && context.User.Identity != null && context.User.Identity.IsAuthenticated && context.User.Identity.Name != null)
            {
                return context.User.Identity.Name;
            }

            return "GUEST";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(Exception ex)
            {
                string sInsBy = GetUserName(context);
                StackTrace stackTrace = new StackTrace(ex, true);
                StackFrame? frame = stackTrace.GetFrame(0);
                string? fileName = frame?.GetFileName();
                int? lineNumber = frame?.GetFileLineNumber();
                string? methodName = frame?.GetMethod()?.DeclaringType?.Name;
                string? sEventCatg = Path.GetFileName(fileName);
                string sEventMsg = $"{ex.Message}\t{ex.InnerException?.Message}";
                string sEventSrc = methodName != null ? methodName.Substring(0, methodName.LastIndexOf(">") + 1) + "\tLine:" + lineNumber : "Unknown";
                string sEventType = context.Request.Method;

                await TraceLog(sEventCatg, sEventMsg, sEventSrc, sEventType, sInsBy);
                await HandleExceptionAsync(context, ex);
            }
        }

        public async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            var statusCode = context.Response.StatusCode;
            string errorMessage = ex.Message;


            ErrorDetails err = new ErrorDetails()
            {
                Message = errorMessage,
                StatusCode = statusCode
            };

            await context.Response.WriteAsync(err.ToString());
        }

        public async Task TraceLog(string? sEventCatg, string sEventMsg, string? sEventSrc, string sEventType, string sInsBy)
        {
            string sTraceTime = DateTime.Now.ToString("ddMMMyyyyHH");
            string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==>\t";

            string sTraceMsg = $"{sEventType, -6}\t{sEventCatg, -40}\t{sEventSrc, -40}\t{sInsBy, -40}\t{sEventMsg}\n";

            string loggingFolder = Path.Combine(Directory.GetCurrentDirectory(), "Logging/Exceptions");
            if (!Directory.Exists(loggingFolder))
            {
                Directory.CreateDirectory(loggingFolder);
            }

            string lstPathSeparator = Path.DirectorySeparatorChar.ToString();
            string lstMonth = DateTime.Now.Month < 10
                                         ? "0" + DateTime.Now.Month.ToString()
                                         : DateTime.Now.Month.ToString();
            string lstYear = DateTime.Now.Year.ToString();
            string lstDestination = loggingFolder + lstPathSeparator + lstYear + lstMonth + lstPathSeparator + DateTime.Now.ToString("ddMMM") + lstPathSeparator;
            if (!Directory.Exists(lstDestination))
                Directory.CreateDirectory(lstDestination);
            string sPathName = lstDestination + lstPathSeparator + sTraceTime + ".txt";
            StreamWriter sw = new StreamWriter(sPathName, true);
            await sw.WriteLineAsync(sLogFormat + sTraceMsg);
            sw.Flush();
            sw.Close();
        }
    }

    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = new string("");
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
