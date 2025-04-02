using AsyncPlayground.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AsyncPlayground
{
	internal class Application
	{
		private Dictionary<string, int> _cache = new() { ["x"] = 42 };
		private readonly ApplicationContext _context;

		public Application(ApplicationContext context)
		{
			_context = context;
		}

        public async Task Go()
        {
            Task1_WriteHelloWorld();
            await Task2_AddEmployeeAsync();
            await Task3_ExceptionAsync();
            await Task4_ReturnEmployeesAsyns();
            await Task5_DisposalAsync();
            await Task6_MultipleCallsAsync();
            await Task7_1_GetFirstEmployeeNameAsync();
            await Task7_2_CorrectBlockingAsync();		
            await Task8_FireAndForgetAsync();
            //Task8_FireAndForgetForReal();
            await Task9_GetCachedValueAsync();

            using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(11))) 
            {
                await Task10_CancellationAsync(cancellationTokenSource.Token);
            }

            Console.ReadKey();
        }

        // Task 1. Unnecessary State Machine involved.
        // What done: removed async couse it is not needed.
        private void Task1_WriteHelloWorld()
        {
            Console.WriteLine("Hello World!");
        }

        // Task 2. Everything looks fine... or does it?
        // What done: added await to log ex.Message
        private async Task<int> Task2_AddEmployeeAsync()
        {
            try
            {
                return await CreateEmployee();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private Task<int> CreateEmployee()
        {
            // Add a new employee to the database
            var employee = new Employee
            {
                Name = "Jon Snow",
                Department = "IT"
            };
            _context.Employees.Add(employee);
            return _context.SaveChangesAsync(); //didn't add await because I added it on CreateEmployee() call.
                                                //Different articles has different points of view according this.
        }

        // Task 3. We should see the exception message in the output
        //What done: change AsyncVoidMethodThrowsException and CatchTheException from void to async Task
        private Task Task3_ExceptionAsync()
        {
            return CatchExceptionAsync();
        }

        private async Task CatchExceptionAsync()
        {
            try
            {
                await ThrowExceptionAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new Exception("Hmmm, something went wrong!");
        }

        // Task 4. Return the list of employees
        // What done: removed extra await inside ReturnEmployeesListAsync, but left in this method according to task.
        private async Task<List<Employee>> Task4_ReturnEmployeesAsyns()
        {
            return await ReturnEmployeesListAsync();
        }

        private Task<List<Employee>> ReturnEmployeesListAsync()
        {
            return _context.Employees.ToListAsync();
        }

        // Task 5. Avoid early disposal
        // What done: made ReturnTaskToReadFileEarlyDisposeAsync async and added await for ReadToEndAsync()
        private async Task Task5_DisposalAsync()
        {
            var result = await ReturnTaskToReadFileEarlyDisposeAsync();
            Console.WriteLine("Task5 " + result);
        }

        public async Task<string> ReturnTaskToReadFileEarlyDisposeAsync()
        {
            using (var reader = new StreamReader("config.json"))
            {
                return await reader.ReadToEndAsync();
            }
        }

        // Task 6. Optimize multiple calls that are not dependent on each other
        //What done: used WhenAll();
        //WaitAll() that mentioned in article may block thread.
        private async Task<List<Employee>> Task6_MultipleCallsAsync()
        {
            var departmentTasks = new[]
            {
                GetEmployeesFromDepartmentAsync("IT"),
                GetEmployeesFromDepartmentAsync("Financial"),
                GetEmployeesFromDepartmentAsync("BI")
            };

            List<Employee>[] employeesFromDepartments = await Task.WhenAll(departmentTasks);
            return employeesFromDepartments.SelectMany(x => x).ToList();
        }

		private async Task<List<Employee>> GetEmployeesFromDepartmentAsync(string department)
		{
			return await _context.Employees.Where(e => e.Department == department).ToListAsync();
		}

        // Task 7.1. I just don't like AggregateExceptions
        //What done: changed method to async Task and added await
        private async Task<string> Task7_1_GetFirstEmployeeNameAsync()
        {
            return await GetFirstEmployeeNameAsync();
        }

        private async Task<string> GetFirstEmployeeNameAsync()
        {
            return (await _context.Employees.FirstOrDefaultAsync())?.Name;
        }

        // Task 7.2. I just don't like AggregateExceptions
        //What done: changed method to async Task and added await
        private async Task Task7_2_CorrectBlockingAsync()
        {
            await LongImportantJobThatShouldBeAwaited();
        }

		private async Task LongImportantJobThatShouldBeAwaited()
		{
			await Task.Delay(5000);
		}

        // Task 8. Avoid Fire-and-Forget Without Logging or Handling
        //What done: make method async and await task 
        private async Task Task8_FireAndForgetAsync()
        {
            try
            {
                await DoBackgroundWorkAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Task8: {ex.Message}");
            }
        }

        private async Task DoBackgroundWorkAsync()
        {
            await Task.Delay(1000);
            Console.WriteLine("Background work completed!");
            throw new Exception("Background work failed!");
        }
        
        private void Task8_FireAndForgetForReal()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await DoBackgroundWorkAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Task 8. Real forget: {ex.Message}");
                }
            });
        }

        // Task 9: This method is being called really often. Try to optimize its memory consumption.
        //What done: changed Task<int> to ValueTask<int>
        public async ValueTask<int> Task9_GetCachedValueAsync()
        {
            if (_cache.TryGetValue("x", out var value))
                return value;

			return await FetchValueAsync();
		}

		private async Task<int> FetchValueAsync()
		{
			await Task.Delay(100);
			return new Random().Next();
		}

        // Task 10. Provide cancellation mechanism for the long-running task
        //Waht done: added CancellationToken
        private async Task Task10_CancellationAsync(CancellationToken cancellationToken)
        {
            await LongRunningTaskAsync(cancellationToken);
        }

        public async Task LongRunningTaskAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < 100; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine("Task 10 canceled");
                    break;
                }
                
                await Task.Delay(5000);

                /** Or in case with Delay:
                try
                {
                    await Task.Delay(5000, cancellationToken);
                    Console.WriteLine(DateTime.UtcNow);
                }
                catch (TaskCanceledException ex)
                {
                    Debug.WriteLine("Task 10 canceled");
                    break;
                }
                **/
            }
        }
    }
}
