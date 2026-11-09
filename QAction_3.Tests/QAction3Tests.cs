using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.UnitTestingFramework.Protocol;

namespace QAction_3.Tests
{
    [TestClass]
    public class QAction3Tests
    {
        private const string InterfaceKey = "1";

        private SLProtocolMock protocolMock;

        [TestInitialize]
        public void Initialize()
        {
            string protocolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "protocol.xml");
            protocolMock = new SLProtocolMock(protocolPath);
            protocolMock.Setup(protocol => protocol.RowKey()).Returns(InterfaceKey);
        }

        [TestMethod]
        public void Run_WhenIfSpeedIsBelowUInt32Max_CopiesIfSpeedToRetrievedColumn()
        {
            const double ifSpeed = 1000000000d;
            AddIfTableRow(ifSpeed, null);

            QAction.Run(protocolMock.Object);

            Assert.AreEqual(ifSpeed, GetRetrievedSpeed());
        }

        [TestMethod]
        public void Run_WhenIfSpeedIsUInt32Max_CopiesIfHighSpeedToRetrievedColumn()
        {
            const double ifHighSpeed = 10000d;
            AddIfTableRow((double)UInt32.MaxValue, null);
            AddIfXTableRow(ifHighSpeed);

            QAction.Run(protocolMock.Object);

            Assert.AreEqual(ifHighSpeed, GetRetrievedSpeed());
        }

        [TestMethod]
        public void Run_WhenIfXTableRowsAreOutOfOrder_UsesMatchingInterfaceKey()
        {
            AddIfTableRow("1", 1000000000d, null);
            AddIfTableRow("2", (double)UInt32.MaxValue, null);
            AddIfTableRow("3", (double)UInt32.MaxValue, null);
            AddIfXTableRow("3", 30000d);
            AddIfXTableRow("2", 20000d);

            QAction.Run(protocolMock.Object);

            Assert.AreEqual(1000000000d, GetRetrievedSpeed("1"));
            Assert.AreEqual(20000d, GetRetrievedSpeed("2"));
            Assert.AreEqual(30000d, GetRetrievedSpeed("3"));
        }

        [TestMethod]
        public void Run_WhenIfSpeedIsMissing_DoesNotWriteRetrievedSpeed()
        {
            const double existingRetrievedSpeed = 123d;
            AddIfTableRow(null, existingRetrievedSpeed);

            QAction.Run(protocolMock.Object);

            Assert.AreEqual(existingRetrievedSpeed, GetRetrievedSpeed());
        }

        [TestMethod]
        public void Run_WhenExtendedInterfaceRowIsMissing_DoesNotWriteRetrievedSpeed()
        {
            const double existingRetrievedSpeed = 123d;
            AddIfTableRow((double)UInt32.MaxValue, existingRetrievedSpeed);

            QAction.Run(protocolMock.Object);

            Assert.AreEqual(existingRetrievedSpeed, GetRetrievedSpeed());
        }

        [TestMethod]
        public void Run_WhenIfHighSpeedIsMissing_DoesNotWriteRetrievedSpeed()
        {
            const double existingRetrievedSpeed = 123d;
            AddIfTableRow((double)UInt32.MaxValue, existingRetrievedSpeed);
            AddIfXTableRow(null);

            QAction.Run(protocolMock.Object);

            Assert.AreEqual(existingRetrievedSpeed, GetRetrievedSpeed());
        }

        private void AddIfTableRow(object ifSpeed, object retrievedSpeed)
        {
            AddIfTableRow(InterfaceKey, ifSpeed, retrievedSpeed);
        }

        private void AddIfTableRow(string interfaceKey, object ifSpeed, object retrievedSpeed)
        {
            protocolMock.Object.AddRow(
                Parameter.Iftable.tablePid,
                new object[] { interfaceKey, null, ifSpeed, null, retrievedSpeed });
        }

        private void AddIfXTableRow(object ifHighSpeed)
        {
            AddIfXTableRow(InterfaceKey, ifHighSpeed);
        }

        private void AddIfXTableRow(string interfaceKey, object ifHighSpeed)
        {
            protocolMock.Object.AddRow(
                Parameter.Ifxtable.tablePid,
                new object[] { interfaceKey, ifHighSpeed });
        }

        private object GetRetrievedSpeed()
        {
            return GetRetrievedSpeed(InterfaceKey);
        }

        private object GetRetrievedSpeed(string interfaceKey)
        {
            return protocolMock.Assert().Table(Parameter.Iftable.tablePid).Row(interfaceKey)[4];
        }
    }
}
