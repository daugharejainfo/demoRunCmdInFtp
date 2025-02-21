using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InstallNpm.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class NodeIntallController : ControllerBase
    {
        [HttpPost("Install")]
        public IActionResult InstallNode()
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine("tools", "nodejs", "node"), // Linux: "node", Windows: "node.exe"
                    Arguments = "your-script.js",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var process = Process.Start(processStartInfo))
                {
                    process?.WaitForExit();

                    if (process is { ExitCode: 0 })
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        return Ok($"Node installed successfully!\n{output}");
                    }
                    else
                    {
                        var error = process.StandardError.ReadToEnd();
                        return StatusCode(500, $"Failed to install node'.\n{error}");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}