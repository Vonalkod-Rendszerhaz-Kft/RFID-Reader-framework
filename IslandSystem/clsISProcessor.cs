using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Log4Pro.IS.TRM.EventHubContract;
using Vrh.EventHub.Core;
using Vrh.EventHub.Protocols.RedisPubSub;

namespace IslandSystemProcessor
{

	public class clsVNTItem
	{
		public string sId = "";
		public string sAnt = "";
		public string sRSSI = "";
		public DateTime dtLeolvasas = DateTime.MinValue;

	}

	public class clsVNTPackage
	{
		public clsVNTPackage(int _PackageId, int _MezoMeter)
		{
			uidPackage = _PackageId;
			iMezoMeter = _MezoMeter;
		}

		public List<clsVNTItem> colItems = new List<clsVNTItem>();

		public int iMezoMeter = 2;
		public int uidPackage = 0;

		public bool bMezoA = false;
		public bool bMezoAHit = false;
		public string sMezoATime = "";
		public string sMezoAEndTime = "";
		public int iMezoATime = 0;
		public int iMezoAEndTime = 0;
		public bool bMezoB = false;
		public bool bMezoBHit = false;
		public string sMezoBTime = "";
		public string sMezoBEndTime = "";
		public int iMezoBTime = 0;
		public int iMezoBEndTime = 0;

		public DateTime dtMezoKezd = DateTime.MinValue;
		public DateTime dtMezoVege = DateTime.MinValue;

		public int iIrany = 0;

		public string sReaderIP = "";
		public Guid uidReader;

		public DateTime dtElsoMessage = DateTime.MinValue;
		public DateTime dtUtolsoMessage = DateTime.MinValue;

		public string sTargoncaAzon = "";

		public bool bArchiveLater = false;

		public DateTime dtErrorLogged = DateTime.MaxValue;

	}

	public class clsVNTLog
	{
		public DateTime dtMessage = DateTime.Now;
		public string sReaderIP = "";
		public int iErrorCode = 0;
		public string sErrorMessage = "";
		public string RfidS = "";
	}

	public class clsISProcessor : VRHReaderFrameworkCommon.clsProcessorBase
	{
		private static object lockThread = new object();
		private static object lockData = new object();
		private static object lockLog = new object();

		private static List<clsVNTPackage> colPackages = new List<clsVNTPackage>();
		private static List<clsVNTLog> colVNTLog = new List<clsVNTLog>();

		private System.Threading.Thread oThread = new System.Threading.Thread(ProcessorThread);

		public static int iDataOffset = 4;
		public static int iPackageTimeoutSec = 10;
		public static string sTargoncaElotag = "VRT";
		public static string sCsEElotag = "VRC";
		public static string sRSSIValueName = "rssi";
		public static string sANTValueName = "ant";
		public static int iEventABTimeLimit = 5;
		public static string SQLConnectString = "";

		private static int ReadMezoMeter(string sReaderIP)
		{
			int iRet = 5;
			
			return iRet;
		}

		private static bool ArchivePackage(clsVNTPackage oPackage)
		{
			bool bRet = false;
			bool bWasError = false;
			try
			{
				if (oPackage.colItems.Count == 0)
				{
					clsVNTItem oItem = new clsVNTItem();
					oItem.dtLeolvasas = oPackage.dtElsoMessage;
					oItem.sAnt = "";
					oItem.sRSSI = "";
					oItem.sId = oPackage.sTargoncaAzon;
					oPackage.colItems.Add(oItem);
				}

				foreach (clsVNTItem oItem in oPackage.colItems)
				{
					Debug.WriteLine($"INSERT INTO [CSEJournals] ([RFID],[Gate],[PostTime],[State],[Message],[UserName],[ReadCycleId],[rssi]) VALUES ({oItem.sId},{oPackage.sReaderIP},{oItem.dtLeolvasas},{oPackage.iIrany},{DBNull.Value},{oPackage.sTargoncaAzon},{oPackage.uidPackage},{oItem.sRSSI})");

					if (oPackage.iIrany == 2)
					{
						bWasError = !PutOut(oItem.sId);
					}
					else if (oPackage.iIrany == 3)
					{
						bWasError = !Receive(oItem.sId);
					}
					bRet = true;
				}
				
			}
			catch (Exception e)
			{
				VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
			}

			return bRet;
		}

		private static bool ArchiveLogs()
		{
			bool bRet = false;

			lock (lockLog)
			{
				try
				{

					List<clsVNTLog> colSavedLogItems = new List<clsVNTLog>();

					foreach (clsVNTLog oLog in colVNTLog)
					{
						Debug.WriteLine($"INSERT INTO [RFIDLogs] ([Gate],[EventTime],[ErrorCode],[ErrorMessage],[RfidS]) VALUES ({oLog.sReaderIP},{oLog.dtMessage},{oLog.iErrorCode},{oLog.sErrorMessage},{oLog.RfidS})");
						colSavedLogItems.Add(oLog);
					}

					foreach (clsVNTLog oLog in colSavedLogItems)
					{
						colVNTLog.Remove(oLog);
					}
				}
				catch (Exception e)
				{
					VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
				}
			}

			return bRet;
		}

		private static void AddVNTLog(clsVNTPackage oPackage, string sMessage, int iErrorCode)
		{
			lock (lockLog)
			{
				try
				{
					clsVNTLog oLog = new clsVNTLog();
					oLog.iErrorCode = iErrorCode;
					oLog.sErrorMessage = sMessage;

					if (oPackage != null)
					{
						oLog.sReaderIP = oPackage.sReaderIP;

						oLog.RfidS += oPackage.sTargoncaAzon;

						foreach (clsVNTItem oItem in oPackage.colItems)
						{
							oLog.RfidS += ";" + oItem.sId;

						}
					}

					colVNTLog.Add(oLog);
				}
				catch (Exception e)
				{
					VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
				}
			}
		}

		private static void ProcessorThread(object oParam)
		{
			while (true)
			{
				lock (lockData)
				{
					try
					{
						List<clsVNTPackage> removePackages = new List<clsVNTPackage>();
						List<clsVNTPackage> archivePackages = new List<clsVNTPackage>();

						foreach (clsVNTPackage oPackage in colPackages)
						{
							if (oPackage.dtUtolsoMessage != DateTime.MinValue)
							{
								if ((DateTime.Now - oPackage.dtUtolsoMessage).TotalSeconds > iPackageTimeoutSec)
								{
									if (oPackage.bArchiveLater)
									{
										archivePackages.Add(oPackage);
									}
									else
									{
										removePackages.Add(oPackage);
									}
								}
							}
						}

						foreach (clsVNTPackage oPackage in archivePackages)
						{
							if (ArchivePackage(oPackage))
							{
								colPackages.Remove(oPackage);

								AddVNTLog(oPackage, "Mentett csomag törölve.", 0);

								string sLog = "Mentett csomag törölve! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
								VRHReaderFrameworkCommon.clsLogger.Info(sLog);
							}
						}

						foreach (clsVNTPackage oPackage in removePackages)
						{
							colPackages.Remove(oPackage);

							AddVNTLog(oPackage, "Lejárt csomag törölve.", 0);

							string sLog = "Lejárt csomag törölve! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
							VRHReaderFrameworkCommon.clsLogger.Info(sLog);
						}
					}
					catch (Exception e)
					{
						VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
					}
				}

				ArchiveLogs();

				System.Threading.Thread.Sleep(1000);
			}
		}


		public override void LoadConfig(string name)
		{
			System.Xml.XmlDocument oXmlDoc = new System.Xml.XmlDocument();

			try
			{
				oXmlDoc.Load(name);
			}
			catch
			{
				using (System.IO.StreamReader oReader = new System.IO.StreamReader(name, System.Text.Encoding.GetEncoding(1250)))
				{
					oXmlDoc.Load(oReader);
				}
			}

			foreach (System.Xml.XmlNode node in oXmlDoc.DocumentElement.ChildNodes)
			{
				if (string.Compare(node.Name, "SQLConnectString", true) == 0)
				{
					SQLConnectString = node.InnerText;
				}
				else if (string.Compare(node.Name, "iDataOffset", true) == 0)
				{
					iDataOffset = int.Parse(node.InnerText);
				}
				else if (string.Compare(node.Name, "iPackageTimeoutSec", true) == 0)
				{
					iPackageTimeoutSec = int.Parse(node.InnerText);
				}
				else if (string.Compare(node.Name, "sTargoncaElotag", true) == 0)
				{
					sTargoncaElotag = node.InnerText;
				}
				else if (string.Compare(node.Name, "sCsEElotag", true) == 0)
				{
					sCsEElotag = node.InnerText;
				}
				else if (string.Compare(node.Name, "sRSSIValueName", true) == 0)
				{
					sRSSIValueName = node.InnerText;
				}
				else if (string.Compare(node.Name, "sANTValueName", true) == 0)
				{
					sANTValueName = node.InnerText;
				}
				else if (string.Compare(node.Name, "iEventABTimeLimit", true) == 0)
				{
					iEventABTimeLimit = int.Parse(node.InnerText);
				}

			}

			oXmlDoc = null;
		}

		public bool ValidateStartAndStop(clsVNTPackage oPackage)
		{
			bool bRet = true;

			if (oPackage.iMezoATime != 0 && oPackage.iMezoBTime != 0 && oPackage.iMezoAEndTime != 0 && oPackage.iMezoBEndTime != 0)
			{
				if ((Math.Abs(oPackage.iMezoATime - oPackage.iMezoBTime) < iEventABTimeLimit) && (Math.Abs(oPackage.iMezoAEndTime - oPackage.iMezoBEndTime) < iEventABTimeLimit))
				{
					bRet = false;
					string sLog = "A start és a stop időpontok is a tűréshatéron belül vannak! A targonca nem haladt át a kapun! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
					VRHReaderFrameworkCommon.clsLogger.Info(sLog);
				}
			}
			else
			{
				bRet = false;
				string sLog = "Nincsen meg az összes start és stop időpont! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
				VRHReaderFrameworkCommon.clsLogger.Info(sLog);
			}

			return bRet;
		}

		public int ValidateIrany(clsVNTPackage oPackage)
		{
			int iRet = 0;

			if (oPackage.iMezoATime != 0 && oPackage.iMezoBTime != 0)
			{
				if (Math.Abs(oPackage.iMezoATime - oPackage.iMezoBTime) < iEventABTimeLimit)
				{
					iRet = 3;// 1;
					string sLog = "Megállapított irány --> 1 (3) ! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
					VRHReaderFrameworkCommon.clsLogger.Info(sLog);
				}
				else
				{
					iRet = 2;
					string sLog = "Megállapított irány --> 2! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
					VRHReaderFrameworkCommon.clsLogger.Info(sLog);
				}
			}
			else
			{
				string sLog = "Nincsen meg az összes start időpont az irány megállapításához! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
				VRHReaderFrameworkCommon.clsLogger.Info(sLog);
			}

			return iRet;
		}

		public string ConvertHexToAscii(string hexString, int offset)
		{
			try
			{
				string ascii = string.Empty;

				for (int i = offset; i < hexString.Length; i += 2)
				{
					String hs = string.Empty;

					hs = hexString.Substring(i, 2);
					uint decval = System.Convert.ToUInt32(hs, 16);
					char character = System.Convert.ToChar(decval);
					ascii += character;

				}

				return ascii;
			}
			catch (Exception e)
			{
				VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
			}

			return string.Empty;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="oReadResult"></param>
		/// <returns>
		/// 0 --> Siker
		/// 1 --> Úgy jött adat, hogy nem érkezett még event a mezőktől
		/// 2 --> Mező esemény rendellenesség
		/// 3 --> Azonosító átalakítási hiba
		/// 4 --> Ismeretlen azonosító
		/// 5 --> Többes targonca azonosító
		/// 6 --> Nincsen targonca azonosító, és nincsen leolvasott azonosító
		/// 7 --> Nincsen leolvasott azonosító
		/// 8 --> Nincsen targonca azonosító
		/// 9 --> Valószínűleg nem ment át a targonca
		/// -1 --> Olvasási csomag mentve
		/// -2 --> Olvasási csomag mentésre jelölve
		/// -3 --> Mezőbe lépés
		/// </returns>
		public int ProcessItem(VRHReaderFrameworkCommon.clsReadResult oReadResult)
		{
			int iRet = 0;

			lock (lockData)
			{
				try
				{
					if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data
						|| oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent
						|| oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Event)
					{
						clsVNTPackage oPackage = colPackages.FirstOrDefault(x => x.uidReader == oReadResult.uidReader);

						if (oPackage == null)
						{
							oPackage = new clsVNTPackage(0, ReadMezoMeter(oReadResult.oReaderThreadConfig.readerip));
							oPackage.dtElsoMessage = DateTime.Now;
							oPackage.uidReader = oReadResult.uidReader;
							oPackage.sReaderIP = oReadResult.oReaderThreadConfig.readerip;
							colPackages.Add(oPackage);
						}

						oPackage.dtUtolsoMessage = DateTime.Now;

						if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data
							|| oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent)
						{
							if (oPackage.bMezoAHit || oPackage.bMezoBHit)
							{ // Ha valamelyik mező már felment
								string sId = ConvertHexToAscii(oReadResult.sResult, iDataOffset);

								if (!string.IsNullOrEmpty(sId))
								{
									if (sId.StartsWith(sTargoncaElotag))
									{
										if (string.IsNullOrEmpty(oPackage.sTargoncaAzon))
										{
											oPackage.sTargoncaAzon = sId;
										}
										else if (oPackage.sTargoncaAzon != sId)
										{
											iRet = 5;
											string sLog = "Többes targonca azonosító! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon + "/" + sId;
											VRHReaderFrameworkCommon.clsLogger.Info(sLog);
										}
									}
									else if (sId.StartsWith(sCsEElotag))
									{
										clsVNTItem oItem = oPackage.colItems.FirstOrDefault(x => x.sId == sId);

										if (oItem == null)
										{
											oItem = new clsVNTItem();
											oItem.sId = sId;
											oItem.dtLeolvasas = oReadResult.dtRead;

											if (oReadResult.colSubResults != null)
											{
												foreach (VRHReaderFrameworkCommon.clsReadSubResult oSubResult in oReadResult.colSubResults)
												{
													if (oSubResult.name == sRSSIValueName)
													{
														oItem.sRSSI = oSubResult.value;
													}
													else if (oSubResult.name == sANTValueName)
													{
														oItem.sAnt = oSubResult.value;
													}
												}
											}

											oPackage.colItems.Add(oItem);
										}
									}
									else
									{
										string sLog = "Ismeretlen azonosító! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon + " Azonosito: " + sId;
										VRHReaderFrameworkCommon.clsLogger.Info(sLog);

										//iRet = 4;
									}
								}
								else
								{
									string sLog = "Az azonosító HEX visszafejtése nem sikerült! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon + " HEX Azon: " + oReadResult.sResult;
									VRHReaderFrameworkCommon.clsLogger.Info(sLog);

									//iRet = 3;
								}
							}
							else
							{
								string sLog = "Úgy érkezett adat, hogy nincsen aktív érzékelő mező! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
								VRHReaderFrameworkCommon.clsLogger.Info(sLog);

								iRet = 1;
							}
						}
						else if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Event)
						{
							System.Diagnostics.Debug.WriteLine("EVENT: " + oReadResult.sOriginalResult + "\n");
							if (oReadResult.colSubResults != null)
							{
								if (oReadResult.colSubResults[1].value == "eIN1")
								{
									if ((oReadResult.colSubResults[4].value == "1" && oPackage.bMezoA == true)
										|| (oReadResult.colSubResults[4].value == "0" && oPackage.bMezoA == false)
										|| (oReadResult.colSubResults[4].value == "1" && oPackage.bMezoAHit == true)
										|| (oReadResult.colSubResults[4].value == "0" && oPackage.bMezoAHit == false))
									{
										iRet = 2;
										string sLog = "Mező 1 hiba! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
										VRHReaderFrameworkCommon.clsLogger.Info(sLog);
									}
									else if (oReadResult.colSubResults[4].value == "1" && oPackage.bMezoA == false)
									{
										oPackage.bMezoA = true;
										oPackage.bMezoAHit = true;
										oPackage.sMezoATime = oReadResult.colSubResults[2].value;
										try
										{
											oPackage.iMezoATime = Int32.Parse(oPackage.sMezoATime, System.Globalization.NumberStyles.HexNumber);
										}
										catch
										{
											oPackage.iMezoATime = 0;
										}

										oPackage.iIrany = ValidateIrany(oPackage);

										AddVNTLog(oPackage, "Mező 1 aktív.", 0);

										string sLog = "Mező 1 --> 1! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
										VRHReaderFrameworkCommon.clsLogger.Info(sLog);

										if (oPackage.dtMezoKezd == DateTime.MinValue)
										{ //Ha ez az első mezőbe lépés
											oPackage.dtMezoKezd = DateTime.Now;
											iRet = -3;
										}
									}
									else if (oReadResult.colSubResults[4].value == "0" && oPackage.bMezoA == true)
									{
										oPackage.bMezoA = false;
										oPackage.sMezoAEndTime = oReadResult.colSubResults[2].value;
										try
										{
											oPackage.iMezoAEndTime = Int32.Parse(oPackage.sMezoAEndTime, System.Globalization.NumberStyles.HexNumber);
										}
										catch
										{
											oPackage.iMezoAEndTime = 0;
										}

										{
											string sLog = "Mező 1 --> 0! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
											VRHReaderFrameworkCommon.clsLogger.Info(sLog);
										}


										if (oPackage.bMezoA == false && oPackage.bMezoB == false && (oPackage.bMezoAHit == true || oPackage.bMezoBHit == true))
										{
											oPackage.dtMezoVege = DateTime.Now;
											iRet = EndOfReading(ref oPackage);
										}
									}
								}
								else if (oReadResult.colSubResults[1].value == "eIN2")
								{
									if ((oReadResult.colSubResults[4].value == "1" && oPackage.bMezoB == true)
										|| (oReadResult.colSubResults[4].value == "0" && oPackage.bMezoB == false)
										|| (oReadResult.colSubResults[4].value == "1" && oPackage.bMezoBHit == true)
										|| (oReadResult.colSubResults[4].value == "0" && oPackage.bMezoBHit == false))
									{
										iRet = 2;

										string sLog = "Mező 2 hiba! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
										VRHReaderFrameworkCommon.clsLogger.Info(sLog);
									}
									else if (oReadResult.colSubResults[4].value == "1" && oPackage.bMezoB == false)
									{
										oPackage.bMezoB = true;
										oPackage.bMezoBHit = true;
										oPackage.sMezoBTime = oReadResult.colSubResults[2].value;
										try
										{
											oPackage.iMezoBTime = Int32.Parse(oPackage.sMezoBTime, System.Globalization.NumberStyles.HexNumber);
										}
										catch
										{
											oPackage.iMezoBTime = 0;
										}

										oPackage.iIrany = ValidateIrany(oPackage);

										AddVNTLog(oPackage, "Mező 2 aktív.", 0);

										string sLog = "Mező 2 --> 1! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
										VRHReaderFrameworkCommon.clsLogger.Info(sLog);

										if (oPackage.dtMezoKezd == DateTime.MinValue)
										{ // Ha ez az első mezőbe lépés
											oPackage.dtMezoKezd = DateTime.Now;
											iRet = -3;
										}
									}
									else if (oReadResult.colSubResults[4].value == "0" && oPackage.bMezoB == true)
									{
										oPackage.bMezoB = false;
										oPackage.sMezoBEndTime = oReadResult.colSubResults[2].value;
										try
										{
											oPackage.iMezoBEndTime = Int32.Parse(oPackage.sMezoBEndTime, System.Globalization.NumberStyles.HexNumber);
										}
										catch
										{
											oPackage.iMezoBEndTime = 0;
										}

										{
											string sLog = "Mező 2 --> 0! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
											VRHReaderFrameworkCommon.clsLogger.Info(sLog);
										}

										if (oPackage.bMezoA == false && oPackage.bMezoB == false && (oPackage.bMezoAHit == true || oPackage.bMezoBHit == true))
										{
											oPackage.dtMezoVege = DateTime.Now;
											iRet = EndOfReading(ref oPackage);
										}
									}
								}
							}
						}

						if (iRet > 0)
						{
							if (oPackage != null)
							{
								colPackages.Remove(oPackage);

								string sLogMsg = "";

								switch (iRet)
								{
									case 1:
										sLogMsg = "Úgy érkezett adat, hogy nincsen aktív érzékelő mező!";
										break;
									case 2:
										sLogMsg = "Érzékelő mezőhiba!";
										break;
									case 5:
										sLogMsg = "Többes targonca azonosító!";
										break;
									case 6:
										sLogMsg = "Nincsen leolvasott azonosító és targonca azonosító!";
										break;
									case 7:
										sLogMsg = "Nincsen leolvasott azonosító!";
										break;
									case 8:
										sLogMsg = "Nincsen leolvasott targonca azonosító!";
										break;
									case 9:
										sLogMsg = "A targonca valószínűleg nem haladt át mindkét mezőn!";
										break;
									default:
										break;
								}

								AddVNTLog(oPackage, sLogMsg, iRet);

								string sLog = "Hiba miatt az olvasási csomagot töröltük! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
								VRHReaderFrameworkCommon.clsLogger.Info(sLog);
							}
						}
					}
				}
				catch
				{

				}
			}


			return iRet;
		}

		private int EndOfReading(ref clsVNTPackage oPackage)
		{
			int iRet = 0;

			AddVNTLog(oPackage, "Olvasási ciklus vége.", 0);

			if (string.IsNullOrEmpty(oPackage.sTargoncaAzon) && oPackage.colItems.Count == 0)
			{
				iRet = 6;

				string sLog = "Nincsen leolvasott azonosító és targonca azonosító! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
				VRHReaderFrameworkCommon.clsLogger.Info(sLog);
			}
			else if (oPackage.colItems.Count == 0)
			{
				iRet = 7;

				string sLog = "Nincsen leolvasott azonosító! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
				VRHReaderFrameworkCommon.clsLogger.Info(sLog);
			}
			else if (string.IsNullOrEmpty(oPackage.sTargoncaAzon))
			{
				iRet = 8;

				string sLog = "Nincsen leolvasott targonca azonosító! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
				VRHReaderFrameworkCommon.clsLogger.Info(sLog);
			}
			else
			{
				iRet = 0;
			}

			if (iRet == 0 || iRet == 7 || iRet == 8)
			{
				if (oPackage.bMezoAHit == true && oPackage.bMezoBHit == true && ValidateStartAndStop(oPackage))
				{
					try
					{
						int iSebesseg = (int)(oPackage.iMezoMeter / (oPackage.dtMezoVege - oPackage.dtMezoKezd).TotalSeconds);
						AddVNTLog(oPackage, "Targonca sebesség: " + iSebesseg.ToString() + " m/s", 0);
					}
					catch (Exception e)
					{

					}

					if (ArchivePackage(oPackage))
					{
						colPackages.Remove(oPackage);

						AddVNTLog(oPackage, "Mentett csomag törölve.", 0);

						string sLog = "Az olvasási csomag elmentve! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
						VRHReaderFrameworkCommon.clsLogger.Info(sLog);

						iRet = -1;
					}
					else
					{
						oPackage.bArchiveLater = true;
						oPackage.uidReader = Guid.Empty;

						string sLog = "Az olvasási csomag mentésre jelölve! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
						VRHReaderFrameworkCommon.clsLogger.Info(sLog);

						iRet = -2;
					}
				}
				else
				{
					iRet = 9;

					string sLog = "A targonca valószínűleg nem haladt át mindkét mezőn! " + "Reader IP: " + oPackage.sReaderIP + " Targonca: " + oPackage.sTargoncaAzon;
					VRHReaderFrameworkCommon.clsLogger.Info(sLog);
				}
			}

			return iRet;
		}

		public override List<VRHReaderFrameworkCommon.clsAction> Process(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
		{
			List<VRHReaderFrameworkCommon.clsAction> colRet = new List<VRHReaderFrameworkCommon.clsAction>();

			try
			{
				if (oReadResult != null)
				{
					if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data
						|| oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent
						|| oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Event)
					{
						int iProcessingResult = 0;
						lock (lockData)
						{
							try
							{
								iProcessingResult = ProcessItem(oReadResult);
							}
							catch (Exception e)
							{
								VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
							}
						}

						/// -1 --> Olvasási csomag mentve
						/// -2 --> Olvasási csomag mentésre jelölve
						/// -3 --> Mezőbe lépés

						if (iProcessingResult == 0)
						{

						}
						else if (iProcessingResult == -1)
						{
							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now;
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 1 0");
								colRet.Add(oAction);
							}

							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now;
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 2 0");
								colRet.Add(oAction);
							}

							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now;
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 2 1");
								colRet.Add(oAction);
							}

							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now.AddSeconds(10);
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 2 0");
								colRet.Add(oAction);
							}
						}
						else if (iProcessingResult == -2)
						{
							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now;
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 1 0");
								colRet.Add(oAction);
							}

							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now;
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 2 0");
								colRet.Add(oAction);
							}

							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now;
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 2 1");
								colRet.Add(oAction);
							}

							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now.AddSeconds(10);
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 2 0");
								colRet.Add(oAction);
							}


						}
						else if (iProcessingResult == -3)
						{
							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now;
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 1 1");
								colRet.Add(oAction);
							}

							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now;
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 2 1");
								colRet.Add(oAction);
							}

							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now.AddSeconds(1);
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 1 0");
								colRet.Add(oAction);
							}

							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now.AddSeconds(1);
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 2 0");
								colRet.Add(oAction);
							}


						}
						else
						{ // Hiba van
							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now;
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 1 0");
								colRet.Add(oAction);
							}

							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now;
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 2 0");
								colRet.Add(oAction);
							}

							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now;
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 1 1");
								colRet.Add(oAction);
							}

							{
								VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
								oAction.dtAction = DateTime.Now;
								oAction.dtValidAfter = DateTime.Now.AddSeconds(10);
								oAction.iAction = 2;
								oAction.uidReader = oReadResult.uidReader;
								oAction.uidAction = Guid.Empty;
								oAction.uidProcessor = Guid.Empty;
								oAction.colActionParameters = new List<string>();
								oAction.colActionParameters.Add("sMN mDOSetOutput 1 0");
								colRet.Add(oAction);
							}
						}
					}

					lock (lockThread)
					{
						try
						{
							if (oThread.ThreadState == System.Threading.ThreadState.Unstarted)
							{
								oThread.Start(null);
							}
							else if (oThread.ThreadState == System.Threading.ThreadState.Stopped || oThread.ThreadState == System.Threading.ThreadState.Aborted)
							{
								oThread = new System.Threading.Thread(ProcessorThread);
								oThread.Start(null);
							}
						}
						catch (Exception e)
						{
							VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
						}
					}


				}
				oReadResult.eAppProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.Processed;
			}
			catch (Exception e)
			{
				VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
				throw;
			}


			return colRet;
		}

		private static bool PutOut(string packagingUnitId)
		{
			TrackingContract.Response response = null;
			var request = new TrackingContract.PutOutModule.PutOutRequest()
			{
				PackagingUnitId = packagingUnitId
			};
			try
			{
				//response = EventHubCore.Call<RedisPubSubChannel,
				//	TrackingContract.PutOutModule.PutOutRequest,
				//	TrackingContract.Response>($"{TrackingContract.CHANNEL_PREFIX}:{TrackingContract.PutOutModule.MODULE_PREFIX}:demo", request);
				return true;
			}
			catch (Exception ex)
			{
				VRHReaderFrameworkCommon.clsLogger.Error(ex.Message);
				return false;
			}
		}

		private static bool Receive(string shippingUnitId)
		{
			TrackingContract.ReceivingModule.ReceiveResponse response = null;
			var request = new TrackingContract.ReceivingModule.ReceiveRequest()
			{
				ShippingUnitId = shippingUnitId
			};
			try
			{
				//response = EventHubCore.Call<RedisPubSubChannel,
				//	TrackingContract.ReceivingModule.ReceiveRequest,
				//	TrackingContract.ReceivingModule.ReceiveResponse>($"{TrackingContract.CHANNEL_PREFIX}:{TrackingContract.ReceivingModule.MODULE_PREFIX}:demo", request);
				return true;
			}
			catch (Exception ex)
			{
				VRHReaderFrameworkCommon.clsLogger.Error(ex.Message);
				return false;
			}
		}
	}
}
