using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;

namespace InstallNpm.Controller
{
    //  [Route("api/[controller]")]
    //[ApiController]
    public class NodeInstallController : ControllerBase
    {
        private readonly ILogger<NodeInstallController> _logger;

        public NodeInstallController(ILogger<NodeInstallController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Home()
        {
            return Ok("welcome to NodeInstall hear you can setup node installation and run pipeline.");
        }

        [HttpGet("server-info")]
        public IActionResult GetServerInfo()
        {
            return Ok(new
            {
                OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                Architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture,
                CurrentDirectory = Directory.GetCurrentDirectory(),
                BaseDirectory = AppContext.BaseDirectory,
                NodeExists = System.IO.File.Exists(Path.Combine(AppContext.BaseDirectory, "Portable-Node", "node.exe"))
            });
        }

        [HttpGet("InstallNodeWithPerameter")]
        public async Task<IActionResult> InstallNode([FromQuery] string nodeFolder = "node",
            [FromQuery] string nodeExe = "node.exe")
        {
            var basePath = AppContext.BaseDirectory;
            var nodePath = Path.Combine(basePath, nodeFolder, nodeExe);

            _logger.LogInformation("Starting Node.js installation check.");
            _logger.LogDebug("Base directory: {BasePath}", basePath);
            _logger.LogDebug("Node.js expected path: {NodePath}", nodePath);

            // Verify file existence
            if (!System.IO.File.Exists(nodePath))
            {
                _logger.LogError("Node.exe not found at path: {NodePath}", nodePath);
                return StatusCode(500, $"Node.exe not found at: {nodePath}");
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = nodePath,
                Arguments = "-v",
                WorkingDirectory = basePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        _logger.LogError("Failed to start Node.js process.");
                        return StatusCode(500, "Failed to start Node.js process.");
                    }

                    _logger.LogInformation("Node.js process started successfully.");

                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    _logger.LogDebug("Node.js process output: {Output}", output);
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        _logger.LogWarning("Node.js process error: {Error}", error);
                    }

                    if (process.ExitCode == 0)
                    {
                        // Verify version format (e.g., "v20.0.0")
                        if (output.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation("Node.js installed successfully. Version: {Version}", output.Trim());
                            return Ok($"Node.js installed successfully. Version: {output.Trim()}");
                        }

                        _logger.LogError("Node.js check failed. Output: {Output}, Error: {Error}", output, error);
                        return StatusCode(500, $"Node.js check failed. output: {output}, Error: {error}");
                    }

                    _logger.LogError("Node.js process exited with code {ExitCode}. Output: {Output}, Error: {Error}",
                        process.ExitCode, output, error);
                    return StatusCode(500,
                        $"Node.js check failed. output: {output}, Error: {error}, process.ExitCode: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical error occurred while installing Node.js.");
                return StatusCode(500, $"Critical error: {ex.Message}");
            }
        }

        [HttpGet("RunNpmCommandWithFileName")]
        public async Task<IActionResult> RunNpmCommandWithFileName([FromQuery] string npmCommand = "npm install",
            [FromQuery] string fileName = "angular-app/server")
        {
            var basePath = AppContext.BaseDirectory;
            var angularProjectPath = Path.Combine(basePath, fileName);

            if (!Directory.Exists(angularProjectPath))
            {
                _logger.LogError("Angular project directory not found: {Path}", angularProjectPath);
                return BadRequest("Angular project directory not found.");
            }

            _logger.LogInformation("Running command: npm {NpmCommand} in directory: {AngularProjectPath}", npmCommand,
                angularProjectPath);

            /*var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"cd '{angularProjectPath}'; & {{ {npmCommand} }}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = angularProjectPath // ✅ Ensures execution in the correct directory
            };*/
            /*var processStartInfo = new ProcessStartInfo
            {
                FileName = "PowerShell\\pwsh.exe", // Use portable PowerShell executable
                Arguments = $"-NoProfile -Command \"cd '{angularProjectPath}'; {npmCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };*/
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c cd \"{angularProjectPath}\" && {npmCommand}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        _logger.LogError("Failed to start npm process.");
                        return StatusCode(500, "Failed to start npm process.");
                    }

                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    _logger.LogDebug("npm output: {Output}", output.Trim());
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        _logger.LogWarning("npm error: {Error}", error);
                    }

                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation("npm command executed successfully");
                        return Ok($"npm command executed successfully:\n{output.Trim()}");
                    }

                    _logger.LogError("npm command failed. Exit code: {ExitCode}, Error: {Error}", process.ExitCode,
                        error);
                    return StatusCode(500, $"npm command failed. Error:\n{error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical error occurred while running npm.");
                return StatusCode(500, $"Critical error: {ex.Message}");
            }
        }

        [HttpGet("RunNpmCommand")]
        public async Task<IActionResult> RunNpmCommand([FromQuery] string npmCommand = "npm install")
        {
            /*var processStartInfo = new ProcessStartInfo
            {
                FileName = "PowerShell\\pwsh.exe", // Use portable PowerShell executable
                Arguments = $"-NoProfile -Command \"{npmCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };*/
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c  {npmCommand}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        _logger.LogError("Failed to start npm process.");
                        return StatusCode(500, "Failed to start npm process.");
                    }

                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    _logger.LogDebug("npm output: {Output}", output.Trim());
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        _logger.LogWarning("npm error: {Error}", error);
                    }

                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation("npm command executed successfully");
                        return Ok($"npm command executed successfully:\n{output.Trim()}");
                    }

                    _logger.LogError("npm command failed. Exit code: {ExitCode}, Error: {Error}", process.ExitCode,
                        error);
                    return StatusCode(500, $"npm command failed. Error:\n{error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical error occurred while running npm.");
                return StatusCode(500, $"Critical error: {ex.Message}");
            }
        }

        /*
         [HttpGet("SetNodePath")]
        public IActionResult SetNodePath([FromQuery] string nodePath)
        {
            try
            {
                // 🔹 Open Windows Registry to update the system PATH variable
                using (var key = Registry.LocalMachine.OpenSubKey(
                           @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", true))
                {
                    if (key == null)
                        return StatusCode(500, "Failed to open registry key.");

                    // 🔹 Get the current PATH variable
                    var currentPath = key.GetValue("Path", "", RegistryValueOptions.DoNotExpandEnvironmentNames)
                        .ToString();

                    // 🔹 Append the new Node.js path
                    if (!currentPath.Contains(nodePath))
                    {
                        var newPath = $"{nodePath};{currentPath}";
                        key.SetValue("Path", newPath, RegistryValueKind.ExpandString);
                    }
                }

                return Ok(
                    $"✅ Node.js Path Updated Successfully: {nodePath}. Please restart the server for changes to take effect.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Error Updating Node.js Path: {ex.Message}");
            }
        }
         public IActionResult ConfigureRunner()
       {
           string runnerPath = @"C:\home\site\wwwroot\actions-runner-win-x64-2.322.0\config.cmd";
           string githubUrl = "https://github.com/daugharejainfo/demoRunCmdInFtp";
           string token = "eeeee";

           ProcessStartInfo psi = new ProcessStartInfo
           {
               FileName = "cmd.exe",
               Arguments = $"/c \"{runnerPath} --url {githubUrl} --token {token} --unattended\"",
               RedirectStandardOutput = true,
               RedirectStandardError = true,
               UseShellExecute = false,
               CreateNoWindow = true
           };

           Process process = new Process { StartInfo = psi };
           process.Start();

           string output = process.StandardOutput.ReadToEnd();
           string error = process.StandardError.ReadToEnd();

           process.WaitForExit();

           return Ok(new { Output = output, Error = error });
       }


      [HttpGet("run-config")]
       public IActionResult RunConfig([FromQuery] string filePath = "run.cmd")
       {
           try
           {
               /*var basePath = Directory.GetCurrentDirectory();
               //var fileName = "run.cmd"; // Change based on your file
               var runPath = Path.Combine(basePath, "actions-runner-win-x64-2.322.0", filePath);

               if (!Directory.Exists(runPath))
               {
                   _logger.LogError("directory not found: {Path}", runPath);
                   return BadRequest("directory not found.");
               }#1#

               //  string runPath = @"C:\home\site\wwwroot\actions-runner-win-x64-2.322.0\run.cmd";
               string runPath = "D:\\InstallNpm\\InstallNpm\\actions-runner-win-x64-2.322.0\\run.cmd";

               // Log the file path
               _logger.LogInformation($"Runner file path: {runPath}");

               // Check if file exists
               if (!System.IO.File.Exists(runPath))
               {
                   _logger.LogError($"File not found: {runPath}");
                   return NotFound($"File not found: {runPath}");
               }

               _logger.LogInformation("Running command: npm {NpmCommand} in directory: {runPath}",
                   runPath);
               var psi = new ProcessStartInfo
               {
                   /*FileName = "powershell.exe",
                   Arguments =
                       $"-ExecutionPolicy Bypass -NoProfile -Command \"Start-Process -FilePath '{runPath}' -Wait -NoNewWindow\"",#1#
                   FileName = "cmd.exe",
                   Arguments = $"/c \"{runPath}\"",
                   RedirectStandardInput = true,
                   RedirectStandardOutput = true,
                   RedirectStandardError = true,
                   UseShellExecute = false,
                   CreateNoWindow = true
               };

               using var process = new Process();
               process.StartInfo = psi;
               process.Start();
               if (process == null)
               {
                   _logger.LogError("Failed to start  process.");
                   return StatusCode(500, "Failed to start  process.");
               }

               var output = process.StandardOutput.ReadToEnd();
               var error = process.StandardError.ReadToEnd();
               process.WaitForExit();


               _logger.LogInformation($"Command Output: {output}");

               if (!string.IsNullOrEmpty(error))
               {
                   _logger.LogError($"Command Error: {error}");
                   throw new Exception(error);
               }

               _logger.LogInformation("GitHub Runner run successfully.");
               return Ok("GitHub Runner run successfully.");
           }
           catch (Exception ex)
           {
               _logger.LogCritical(ex, "A critical error occurred while running .");
               return StatusCode(500, $"Critical error: {ex.Message}");
           }
       }

       [HttpGet("stop-runner")]
       public IActionResult StopGitHubRunner([FromQuery] string command = "taskkill /F /IM Runner.Listener.exe")
       {
           try
           {
               //RunCmdCommand("taskkill /F /IM Runner.Listener.exe");
               //RunCmdCommand("taskkill /F /IM cmd.exe"); // Stops any scripts running it
               var psi = new ProcessStartInfo
               {
                   FileName = "cmd.exe",
                   Arguments = $"/c {command}",
                   RedirectStandardOutput = true,
                   RedirectStandardError = true,
                   UseShellExecute = false,
                   CreateNoWindow = true
               };

               using (var process = new Process { StartInfo = psi })
               {
                   process.Start();
                   var output = process.StandardOutput.ReadToEnd();
                   var error = process.StandardError.ReadToEnd();
                   process.WaitForExit();
                   _logger.LogInformation($"Command Output: {output}");
                   if (!string.IsNullOrEmpty(error))
                   {
                       _logger.LogError($"Command Error: {error}");
                       throw new Exception(error);
                   }
               } // Stops any scripts running it

               _logger.LogInformation("GitHub Runner stopped successfully.");
               return Ok("GitHub Runner stopped successfully.");
           }
           catch (Exception ex)
           {
               _logger.LogCritical(ex, "A critical error occurred while running npm.");
               return StatusCode(500, $"Critical error: {ex.Message}");
           }
       }


       [HttpGet("get-path")]
       public IActionResult GetFilePath()
       {
           try
           {
               var basePath = Directory.GetCurrentDirectory();
               // var basePath = AppContext.BaseDirectory;
               string fileName = "run.cmd"; // Change based on your file
               string filePath = Path.Combine(basePath, "actions-runner-win-x64-2.322.0", fileName);

               if (!System.IO.File.Exists(filePath))
               {
                   _logger.LogError($"File not found: {filePath}");
                   return NotFound($"File not found: {filePath}");
               }

               _logger.LogInformation($"File path: {filePath}");
               return Ok(new { FilePath = filePath });
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error getting file path.");
               return StatusCode(500, "Internal Server Error");
           }
       }*/
    }
}