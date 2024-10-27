using InstrumentStatusService.Dbml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstrumentStatusService.Controllers
{
    public class InstrumentStatusController : IDisposable
    {
        private const string _connectionString = @"Data Source=ACCESS01;Initial Catalog=dbLSD;User ID=lsduser;Password=./lsd/us3r/;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False";

        private InstrumentStatusDBDataContext _dataContext = null;

        public InstrumentStatusController()
        {
            _dataContext = new InstrumentStatusDBDataContext(_connectionString);
        }

        public IQueryable<tbInstrumentStatus_Directory> GetDirectories()
        {
            return _dataContext.tbInstrumentStatus_Directories;
        }

        public tbInstrumentStatus_Directory GetDirectory(int id)
        {
            return GetDirectories().Where(i => i.id == id).FirstOrDefault();
        }

        public IQueryable<tbInstrumentStatus_Directory> GetDirectories(int instrumentId)
        {
            return GetDirectories().Where(i => i.instrument_id == instrumentId);
        }

        public IQueryable<tbInstrumentStatus_Computer> GetInstruments()
        {
            return _dataContext.tbInstrumentStatus_Computers;
        }

        public IQueryable<tbInstrumentStatus_Computer> GetInstruments(int computerType)
        {
            //grabs all computers by either type dashboard(id 2) or worker(id 1) depending on sent parameter
            return _dataContext.tbInstrumentStatus_Computers.Where(i => i.computertype_id == computerType);
        }

        public tbInstrumentStatus_Computer GetInstrument(int id)
        {
            return GetInstruments().Where(i => i.id == id).FirstOrDefault();
        }

        public tbInstrumentStatus_Computer GetInstrument(string systemName, int computerType)
        {
            return GetInstruments().Where(i => i.system_name == systemName && i.computertype_id == computerType).FirstOrDefault();
        }

        public IQueryable<tbInstrumentStatus_ComputerType> GetComputerTypes()
        {
            return _dataContext.tbInstrumentStatus_ComputerTypes;
        }

        public tbInstrumentStatus_ComputerType GetComputerType(int id)
        {
            return GetComputerTypes().Where(i => i.id == id).FirstOrDefault();
        }

        public tbInstrumentStatus_ComputerType GetComputerType(string name)
        {
            return GetComputerTypes().Where(i => i.name == name).FirstOrDefault();
        }

        public void UpdateLastRan(string systemName, int computerType)
        {
            tbInstrumentStatus_Computer current = GetInstrument(systemName, computerType);

            current.last_ran = DateTime.Now;

            _dataContext.SubmitChanges();
        }

        public bool AddOrUpdateInstrument(tbInstrumentStatus_Computer tbTempInstrument, string type)
        {
            try
            {
                if (tbTempInstrument.id <= 0)
                {
                    tbTempInstrument.id = 0;

                    tbInstrumentStatus_ComputerType tempType = GetComputerType(type);

                    tbTempInstrument.computertype_id = tempType.id;

                    _dataContext.tbInstrumentStatus_Computers.InsertOnSubmit(tbTempInstrument);
                }
                else
                {
                    tbInstrumentStatus_Computer current = GetInstrument(tbTempInstrument.id);

                    current.system_name = tbTempInstrument.system_name;
                    current.runtime_location = tbTempInstrument.runtime_location;
                    current.runtime_version = tbTempInstrument.runtime_version;
                }

                _dataContext.SubmitChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveDirectory(int id)
        {
            try
            {
                _dataContext.tbInstrumentStatus_Directories.DeleteOnSubmit(GetDirectory(id));

                _dataContext.SubmitChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AddOrUpdateDirectory(tbInstrumentStatus_Directory tbTempDirectory)
        {
            try
            {
                if (tbTempDirectory.id <= 0)
                {
                    tbTempDirectory.id = 0;

                    _dataContext.tbInstrumentStatus_Directories.InsertOnSubmit(tbTempDirectory);
                }
                else
                {
                    tbInstrumentStatus_Directory current = GetDirectory(tbTempDirectory.id);

                    current.file_wildcard = tbTempDirectory.file_wildcard;
                    current.directory_location = tbTempDirectory.directory_location;
                    current.injection_interval = tbTempDirectory.injection_interval;
                }

                _dataContext.SubmitChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool FileModified(int directoryId, FileInfo fileInfo)
        {
            try
            {
                tbInstrumentStatus_Directory tbTempDirectory = GetDirectory(directoryId);

                tbTempDirectory.last_file_modified_date = fileInfo.LastWriteTime;
                tbTempDirectory.last_file_modified_name = fileInfo.Name;

                _dataContext.SubmitChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _dataContext.Dispose();
       }
    }
}
