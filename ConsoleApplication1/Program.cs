using ConsoleApplication1.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleApplication1
{
    static class Program
    {
        static ILogger _logger;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            Console.WriteLine("Начальная загрузка");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            try
            {
                if (Environment.UserInteractive)
                {
                    //_logger.Info("Подготовка консоли");
                    return ProcessingConsole(args);
                }
                else
                {
                    _logger = LogManager.GetLogger("KudaGoService");
                    _logger.WriteSartLine("Запуск службы");
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new KudaGoService()
                    };
                    ServiceBase.Run(ServicesToRun);
                    _logger.Info("Работа службы завершена");
                    return 0;
                }
            } catch(Exception ex)
            {
                _logger.Error(ex);
                return -1;
            }
            finally
            {
                _logger.Info("Закрытие логгера");
                LogManager.Flush();
                LogManager.Shutdown();
            }
        }

        //запуск консоли
        private static int ProcessingConsole(string[] args)
        {
            //добавляем вывод на консоль
            var config = LogManager.Configuration;
            var logTarget = new NLog.Targets.ColoredConsoleTarget("Console");
            logTarget.Layout = new NLog.Layouts.SimpleLayout("${time} ${level:uppercase=true} ${message}");
            config.AddTarget(logTarget);
            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Debug, logTarget));
            LogManager.Configuration = config;
            //получаем логгер для работы
            _logger = LogManager.GetLogger("KudaGoService");
            _logger.WriteSartLine("Запуск приложения");

            //проверяем аргументы на установку/удаление
            string arg = args.FirstOrDefault();
            if (arg != null)
            {
                _logger.Info("Заданы аргументы: {0}", arg);
                args[0] = arg.Replace('-', '/'); //подменяем на стандартные для передачи установщикам службы
                int result;
                switch (arg.ToLower())
                {
                    case "/i":
                    case "-i":  // install
                        result = InstallService(args);
                        new Thread(() =>
                        {
                            MessageBox.Show(result == 0 ? "Служба установлена" : "При установке службы произошла ошибка, подробнее в журнале приложения",
                                "KudaGoService",
                                MessageBoxButtons.OK,
                                result == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Exclamation);
                        }).Start();//отдельный поток для завершения приложения, без накладки с запуском службы
                        break;
                    case "/u":
                    case "-u":  // uninstall
                        result = UninstallService(args);
                        new Thread(() =>
                        {
                            MessageBox.Show(result == 0 ? "Служба удалена" : "При удалении службы произошла ошибка, подробнее в журнале приложения",
                            "KudaGoService",
                            MessageBoxButtons.OK,
                            result == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Exclamation);
                        }).Start();//отдельный поток для завершения приложения, без накладки с запуском службы
                        break;
                    default:  // unknown option
                        _logger.Info("Неизвестный аргумент: {0}", arg);
                        result = -1;
                        break;
                }
                //Console.WriteLine("Для закрытия окна нажмите любую клавишу...");
                //Console.ReadKey();
                return result;
            }

            //запуск для тестирования в консоли
            SyncData();

            Console.WriteLine("Нажимите Esc для выхода");
            while (Console.ReadKey().Key != ConsoleKey.Escape) ;

            //тут нужна сотановка
            _logger.Info("Процесс остановлен");
            LogManager.Flush();
            return 0;
        }

        private static void SyncData()
        {
            using (var db = new KudaGoContext())
            {
                _logger.Info("Подготовка БД");
                db.Database.CreateIfNotExists();
                db.Database.Initialize(true);
                _logger.Info("Подготовка БД завершена");
            }
            _logger.Info("Начало синхронизации");

            var tasks = new[] {
                Task.Run(() => DataLayer.SyncCityListWithKudago()),
                Task.Run(() => DataLayer.SyncCategoryListWithKudago()),
                Task.Run(() => DataLayer.SyncPlaceListWithKudago()),
                Task.Run(() => DataLayer.SyncEventListWithKudago()),
                Task.Run(() => DataLayer.SyncFilmListWithKudago()),
                Task.Run(() => DataLayer.SyncShowListWithKudago())
            };

            Task.WaitAll(tasks);
            _logger.Info("Синхронизация завершена");
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            LogManager.Flush(); //подстраховка на случай падения
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is TaskCanceledException)
            {
                _logger.Warn(e.ExceptionObject as Exception, "Отмена задачи ");
            }
            else
            {
                _logger.Error(e.ExceptionObject as Exception, "Неизвестная ошибка");
            }
        }

        private static int InstallService(string[] args)
        {
            try
            {
                CheckAdmin();
                _logger.Info("Установка службы");
                // install the service with the Windows Service Control Manager (SCM)
                args = args.Concat(new string[] { Assembly.GetExecutingAssembly().Location }).ToArray();
                ManagedInstallerClass.InstallHelper(args);
                _logger.Info("Служба установлена");
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType() == typeof(Win32Exception))
                {
                    Win32Exception wex = (Win32Exception)ex.InnerException;
                    _logger.Error(ex, "Error(0x{0:X}): Service already installed!", wex.ErrorCode);
                    return wex.ErrorCode;
                }
                else
                {
                    _logger.Error(ex);
                    return -1;
                }
            }

            return 0;
        }

        private static int UninstallService(string[] args)
        {
            try
            {
                CheckAdmin();
                _logger.Info("Удаление службы");
                // uninstall the service from the Windows Service Control Manager (SCM)
                args = args.Concat(new string[] { Assembly.GetExecutingAssembly().Location }).ToArray();
                ManagedInstallerClass.InstallHelper(args);
                _logger.Info("Служба удалена");
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType() == typeof(Win32Exception))
                {
                    Win32Exception wex = (Win32Exception)ex.InnerException;
                    _logger.Error(ex, "Error(0x{0:X}): Service not installed!", wex.ErrorCode);
                    return wex.ErrorCode;
                }
                else
                {
                    _logger.Error(ex);
                    return -1;
                }
            }

            return 0;
        }

        private static void CheckAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator) == false)
            {
                throw new Exception("Для выполнения операции необходимы права администратора");
            }
        }
    }

    static class Extensions
    {
        public static void WriteSartLine(this ILogger logger, string message)
        {
            logger.Info(string.Empty);
            logger.Info("------------------{0}---------------------", message);
        }
    }
}
