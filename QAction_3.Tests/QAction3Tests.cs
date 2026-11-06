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
			protocolMock.Object.AddRow(
				Parameter.Iftable.tablePid,
				new object[] { InterfaceKey, null, ifSpeed, null, retrievedSpeed });
		}

		private void AddIfXTableRow(object ifHighSpeed)
		{
			protocolMock.Object.AddRow(
				Parameter.Ifxtable.tablePid,
				new object[] { InterfaceKey, ifHighSpeed });
		}

		private object GetRetrievedSpeed()
		{
			return protocolMock.Assert().Table(Parameter.Iftable.tablePid).Row(InterfaceKey)[4];
		}
	}
}
