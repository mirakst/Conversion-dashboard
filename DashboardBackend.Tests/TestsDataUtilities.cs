using DashboardBackend.Tests.Database;
using Model;
using System;
using System.Collections.Generic;
using Xunit;

namespace DashboardBackend.Tests
{
    public class TestsDataUtilities
    {
        #region GetExecution
        [Fact]
        public void GetExecution_GetsExecutionsFromTestDatabase_ReturnTrue()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();
            var expected = new List<Execution>()
            {
                new Execution(1, DateTime.Parse("01-01-2020 12:00:00")),
                new Execution(2, DateTime.Parse("01-01-2020 13:00:00")),
            };

            var actual = DataUtilities.GetExecutions(DateTime.Parse("01-01-2020 12:00:00"));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetExecution_GetsExecutionsFromTestDatabaseWithNewerMinDateThanLatestExecution_ReturnEmpty()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();

            var actual = DataUtilities.GetExecutions(DateTime.Parse("02-01-2020 12:00:00"));

            Assert.Empty(actual);
        }

        [Fact]
        public void GetExecution_GetsExecutionsFromTestDatabaseWhereExecutionsWithSameIdExists_ReturnExecutions()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();
            var expected = 2;
            var actual = DataUtilities.GetExecutions().FindAll(e => e.Id == 1);

            Assert.True(expected == actual.Count);
        }
        #endregion
        #region GetAfstemninger
        [Fact]
        public void GetAfstemninger_GetsAfstemningerFromTestDatabase_ReturnTrue()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();
            var expected = new List<ValidationTest>()
            {
                new ValidationTest(DateTime.Parse("01-01-2020 12:00:00"),
                                   "validationOne",
                                   ValidationTest.ValidationStatus.Ok,
                                   "managerOne",
                                   0,
                                   0,
                                   0,
                                   "srcSql",
                                   "dstSql"),
                new ValidationTest(DateTime.Parse("01-01-2020 13:00:00"),
                                   "validationTwo",
                                   ValidationTest.ValidationStatus.Ok,
                                   "managerTwo",
                                   0,
                                   0,
                                   0,
                                   "srcSql",
                                   "dstSql")
            };

            var actual = DataUtilities.GetAfstemninger(DateTime.Parse("01-01-2020 12:00:00"));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetAfstemninger_GetsAfstemningerFromTestDatabaseWhereOneAfstemningIsDuplicated_ReturnTrue()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();
            var expected = 2;

            var actual = DataUtilities.GetAfstemninger(DateTime.Parse("01-01-2020 10:00:00")).FindAll(a => a.Name == "validationOne").Count;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetAfstemninger_GetsAfstemningerFromTestDatabase_ThrowsArgumentException()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();

            Assert.Throws<ArgumentException>(() =>DataUtilities.GetAfstemninger(DateTime.Parse("01-01-2020 08:00:00")));
        }

        [Fact]
        public void GetAfstemninger_GetsAfstemningerFromTestDatabaseWhereAfstemningerDateIsOlderThanMinDate_ReturnEmpty()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();

            var actual = DataUtilities.GetAfstemninger(DateTime.Parse("02-01-2020 12:00:00"));

            Assert.Empty(actual);
        }
        #endregion
        #region GetLogMessages
        [Fact]
        public void GetLogMessage_GetsLogMessagesFromTestDatabase_ReturnTrue()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();
            var expected = new List<LogMessage>()
            {
                new LogMessage("Afstemning Error",
                               LogMessage.LogMessageType.Validation | LogMessage.LogMessageType.Error,
                               0,
                               0,
                               DateTime.Parse("01-01-2020 17:00:00")),
                new LogMessage("Check - Error",
                               LogMessage.LogMessageType.Validation | LogMessage.LogMessageType.Error,
                               0,
                               0,
                               DateTime.Parse("01-01-2020 18:00:00")),
                new LogMessage("Info",
                               LogMessage.LogMessageType.Info,
                               0,
                               0,
                               DateTime.Parse("01-01-2020 12:00:00")),
                new LogMessage("Warning",
                               LogMessage.LogMessageType.Warning,
                               0,
                               0,
                               DateTime.Parse("01-01-2020 13:00:00")),
                new LogMessage("Error",
                               LogMessage.LogMessageType.Error,
                               0,
                               0,
                               DateTime.Parse("01-01-2020 14:00:00")),
                new LogMessage("Fatal",
                               LogMessage.LogMessageType.Fatal,
                               0,
                               0,
                               DateTime.Parse("01-01-2020 15:00:00")),
                new LogMessage("None",
                               LogMessage.LogMessageType.None,
                               0,
                               0,
                               DateTime.Parse("01-01-2020 16:00:00")),
            };

            var actual = DataUtilities.GetLogMessages();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetLogMessage_GetsLogMessagesFromTestDatabaseWhereMinDateIsGreaterThanNewestLogMessage_ReturnEmpty()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();

            var actual = DataUtilities.GetLogMessages(DateTime.Parse("02-01-2020 12:00:00"));

            Assert.Empty(actual);
        }

        [Fact]
        public void GetLogMessage_GetsLogMessagesWithSpecificExecutionId_ReturnTrue()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();
            var expected = new List<LogMessage>()
            {
                new LogMessage("Info",
                               LogMessage.LogMessageType.Info,
                               0,
                               0,
                               DateTime.Parse("01-01-2020 12:00:00")),
                new LogMessage("Warning",
                               LogMessage.LogMessageType.Warning,
                               1,
                               0,
                               DateTime.Parse("01-01-2020 13:00:00")),
            };

            var actual = DataUtilities.GetLogMessages(0);

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void GetLogMessage_GetsLogMessagesWithTooLowExecutionIdAndNewerMinDateThanNewestLogMessage_ThrowsArgumentOutOfRangeException()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();

            Assert.Throws<ArgumentOutOfRangeException>(() => DataUtilities.GetLogMessages(-1, DateTime.Parse("02-01-2020 12:00:00")));
        }
        #endregion
        #region GetEstimatedManagerCount
        [Fact]
        public void GetEstimatedManagerCount_ReturnTrue()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();
            var expected = 2;
            var actual = DataUtilities.GetEstimatedManagerCount(0);

            Assert.Equal(expected, actual);
        }
        #endregion
        #region GetManager
        [Fact]
        public void GetManager_GetsManagersFromTestDatabase_ReturnTrue()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();
            var expected = new List<Manager>()
            {
                new Manager()
                {
                    Name = "ManagerOne",
                },
                new Manager()
                {
                    Name = "ManagerTwo",
                },
                new Manager()
                {
                    Name = "ManagerThree",
                }
            };

            var actual = new List<Manager>();
            DataUtilities.GetAndUpdateManagers(DateTime.Parse("01-01-2020 12:00:00"), actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetManager_GetsManagersFromTestDatabaseWhereMinDateIsGreaterThanNewestManager_ReturnEmpty()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();

            var actual = new List<Manager>();
            DataUtilities.GetAndUpdateManagers(DateTime.Parse("02-01-2020 12:00:00"), actual);

            Assert.Empty(actual);
        }
        #endregion
        #region AddManager
        //[Fact]
        //public void AddManager_AddsManagerFromExecutionToManagerList_ReturnTrue()
        //{
        //    DataUtilities.DatabaseHandler = new TestDatabase();
        //    var expected = new List<Manager>()
        //    {
        //        new Manager(1, 1, "ManagerOne"),
        //        new Manager(2, 1, "ManagerTwo"),
        //        new Manager(3, 1, "ManagerThree"),
        //    };

        //    var executionList = DataUtilities.GetExecutions();
        //    DataUtilities.AddManagers(executionList);

        //    var actual = DataUtilities.GetManagers();

        //    Assert.Equal(expected, actual);
        //}
        #endregion
        #region AddHealthReport
        [Fact]
        public void AddHealthReportReadings_GetsReadingsFromTheHealthReport_ReturnTrue()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();
            HealthReport expected = new(string.Empty,
                                        string.Empty,
                                        new Cpu(string.Empty, 69, 420),
                                        new Network(string.Empty, string.Empty, null),
                                        new Ram(100));

            expected.Cpu.Readings = new List<CpuLoad>
            {
                new(1, 0.1, DateTime.Parse("01-01-2020 12:00:00")),
                new(1, 0.2, DateTime.Parse("01-01-2020 13:00:00"))
            };

            expected.Ram.Readings = new List<RamLoad>
            {
                new(1, 0.9, 10, DateTime.Parse("01-01-2020 12:00:00")),
                new(1, 0.8, 20, DateTime.Parse("01-01-2020 13:00:00"))
            };

            expected.Network.Readings = new List<NetworkUsage>
            {
                new(1, 10, 10, 10, 10, 10, 10,DateTime.Parse("01-01-2020 12:00:00")),
                new(1, 20, 20, 20, 20, 20, 20,DateTime.Parse("01-01-2020 13:00:00"))
            };

            HealthReport actual = new(string.Empty,
                                      string.Empty,
                                      new Cpu(string.Empty, 69, 420),
                                      new Network(string.Empty, string.Empty, null),
                                      new Ram(100));

            DataUtilities.AddHealthReportReadings(actual);


            Assert.Equal(expected, actual);
        }
        #endregion
        #region BuildHealthReport
        [Fact]
        public void BuildHealthReport_BuildsAHealthReport_ReturnTrue()
        {
            DataUtilities.DatabaseHandler = new TestDatabase();
            HealthReport expected = new(string.Empty,
                                        string.Empty,
                                        new Cpu("CPU", 100, null),
                                        new Network("Interface", "MAC address", null),
                                        new Ram(null));

            HealthReport actual = new();

            DataUtilities.BuildHealthReport(actual);


            Assert.Equal(expected, actual);
        }
        #endregion
    }
}