
using ConsoleApplication1.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public partial class KudaGoService : ServiceBase
    {
        NLog.ILogger _logger;
        Timer _timer;

        public KudaGoService()
        {
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            _logger = NLog.LogManager.GetLogger("KudaGoService");
            try
            {
                new Thread(SyncData).Start();
                var time = DateTime.Now - DateTime.Today;
                //запускаем круглосуточною синхронизацию 
                _timer = new Timer(SyncData, null, (int)time.TotalMilliseconds, (int)TimeSpan.FromDays(1).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при запуске службы");
                throw;
            }
        }

        private void SyncData(object state)
        {
            try
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
            } catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при выполнении синхронизации");
            }
        }

        protected override void OnStop()
        {
            try
            {
                //тут нужен механизм остановки процессов
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при остановке службы");
                throw;
            }
            finally
            {
                NLog.LogManager.Flush();
            }
        }
    }
}
