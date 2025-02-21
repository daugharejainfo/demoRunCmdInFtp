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
        /*[HttpGet]
        public async Task<IActionResult> InstallNode()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var nodePath = Path.Combine(currentDirectory, "Portable-Node", "node.exe");

// 1. First check if node.exe exists
            if (!System.IO.File.Exists(nodePath))
            {
                return StatusCode(500, "Node.js executable not found - installation failed.");
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = nodePath,
                Arguments = "-v", // Check version
                WorkingDirectory = currentDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true, // Capture both outputs
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        // Verify version format (e.g., "v20.0.0")
                        if (output.StartsWith($"v") || output.StartsWith($"V"))
                        {
                            return Ok($"Node.js installed successfully. Version: {output.Trim()}");
                        }
                        return StatusCode(500, $"Unexpected output: {output}");
                    }
                    else
                    {
                        return StatusCode(500, $"Node.js check failed. Error: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to run Node.js: {ex.Message}");
            }
        }*/

        /*[HttpGet]
        public async Task<IActionResult> InstallNode()
        {
            var basePath = AppContext.BaseDirectory;
            var nodePath = Path.Combine(basePath, "Portable-Node", "node.exe");

            // Verify file existence
            if (!System.IO.File.Exists(nodePath))
            {
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
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        // Verify version format (e.g., "v20.0.0")
                        if (output.StartsWith($"v") || output.StartsWith($"V"))
                        {
                            return Ok($"Node.js installed successfully. Version: {output.Trim()}");
                        }

                        return StatusCode(500, $"Node.js check failed. output: {output}, Error: {error}");
                    }

                    return StatusCode(500, $"Node.js check failed. output: {output}, Error: {error},process.ExitCode : {process.ExitCode}");
                }

                //  Console.WriteLine($"Exit code: {process.ExitCode}");
                Console.WriteLine($"Working directory: {processStartInfo.WorkingDirectory}");
                Console.WriteLine($"Full path: {nodePath}");

                // Rest of your existing logic
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.ToString()}");
                return StatusCode(500, $"Critical error: {ex.Message}");
            }
        }*/


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

        [HttpGet]
        public async Task<IActionResult> InstallNode()
        {
            var basePath = AppContext.BaseDirectory;
            var nodePath = Path.Combine(basePath, "node", "node.exe");

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


        [HttpGet("SetNodePath")]
        public IActionResult SetNodePath([FromQuery] string nodePath)
        {
            try
            {
                // 🔹 Open Windows Registry to update the system PATH variable
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", true))
                {
                    if (key == null)
                        return StatusCode(500, "Failed to open registry key.");

                    // 🔹 Get the current PATH variable
                    string currentPath = key.GetValue("Path", "", RegistryValueOptions.DoNotExpandEnvironmentNames).ToString();
                
                    // 🔹 Append the new Node.js path
                    if (!currentPath.Contains(nodePath))
                    {
                        string newPath = $"{nodePath};{currentPath}";
                        key.SetValue("Path", newPath, RegistryValueKind.ExpandString);
                    }
                }

                return Ok($"✅ Node.js Path Updated Successfully: {nodePath}. Please restart the server for changes to take effect.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Error Updating Node.js Path: {ex.Message}");
            }
        }

        /*[HttpGet("RunNpmCommand")]
        public async Task<IActionResult> RunNpmCommand([FromQuery] string npmCommand = "npm install",
            [FromQuery] string fileName = "angular-app/server")
        {
            var basePath = AppContext.BaseDirectory;
            var angularProjectPath = Path.Combine(basePath, fileName);

            if (!Directory.Exists(angularProjectPath))
            {
                _logger.LogError("Angular project directory not found: {Path}", angularProjectPath);
                return BadRequest("Angular project directory not found.");
            }

            _logger.LogInformation("Running command: {NpmCommand} in directory: {AngularProjectPath}", npmCommand,
                angularProjectPath);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"npm install\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,  // Required for output redirection
                CreateNoWindow = true,
                WorkingDirectory = angularProjectPath  // ✅ Set the working directory correctly
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

                    _logger.LogDebug("npm output: {Output}", output);
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        _logger.LogWarning("npm error: {Error}", error);
                    }

                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation("npm command executed successfully.");
                        return Ok($"npm command executed successfully: {npmCommand}");
                    }

                    _logger.LogError("npm command failed. Exit code: {ExitCode}, Error: {Error}", process.ExitCode,
                        error);
                    return StatusCode(500, $"npm command failed. Error: {error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical error occurred while running npm.");
                return StatusCode(500, $"Critical error: {ex.Message}");
            }
            /*var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"cd '{angularProjectPath}'; {npmCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false, // Required for output redirection
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

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    _logger.LogDebug("npm output: {Output}", output);
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        _logger.LogWarning("npm error: {Error}", error);
                    }

                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation("npm command executed successfully.");
                        return Ok($"npm command executed successfully:\n{output}");
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
            }#1#
        }*/

        [HttpGet("RunNpmCommandWithFilePath")]
        public async Task<IActionResult> RunNpmCommand([FromQuery] string npmCommand = "npm install",
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
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "PowerShell\\pwsh.exe", // Use portable PowerShell executable
                Arguments = $"-NoProfile -Command \"cd '{angularProjectPath}'; {npmCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            /*var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c cd \"{angularProjectPath}\" && {npmCommand}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };*/

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        _logger.LogError("Failed to start npm process.");
                        return StatusCode(500, "Failed to start npm process.");
                    }

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
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
            var basePath = AppContext.BaseDirectory;
           // var angularProjectPath = Path.Combine(basePath, fileName);

            /*
            if (!Directory.Exists(angularProjectPath))
            {
                _logger.LogError("Angular project directory not found: {Path}", angularProjectPath);
                return BadRequest("Angular project directory not found.");
            }

            _logger.LogInformation("Running command: npm {NpmCommand} in directory: {AngularProjectPath}", npmCommand,
                angularProjectPath);
                */

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
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "PowerShell\\pwsh.exe", // Use portable PowerShell executable
                Arguments = $"-NoProfile -Command \"{npmCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            /*var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c cd \"{angularProjectPath}\" && {npmCommand}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };*/

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        _logger.LogError("Failed to start npm process.");
                        return StatusCode(500, "Failed to start npm process.");
                    }

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
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
        
        
            [HttpGet("run-config")]
            public IActionResult RunConfig([FromQuery] string filePath="actions-runner-win-x64-2.322.0\\config.cmd")
            {
                try
                {
                    var basePath = Directory.GetCurrentDirectory();
                    var runPath = Path.Combine(basePath, filePath);
                    
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-ExecutionPolicy Bypass -NoProfile -Command \"Start-Process -FilePath '{runPath}' -Wait -NoNewWindow\"",  
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = new Process { StartInfo = psi })
                    {
                        process.Start();
                        using (StreamWriter sw = process.StandardInput)
                        {
                            sw.Write("");
                            sw.Flush();
                        }

                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(error))
                        {
                            throw new Exception(error);
                        }
                    }

                    return Ok("Config files executed successfully.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error: {ex.Message}");
                }
            }

        }

    }
