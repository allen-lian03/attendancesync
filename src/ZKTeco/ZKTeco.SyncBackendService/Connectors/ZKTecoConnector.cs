using System.Threading;
using Topshelf.Logging;
using ZKTeco.SyncBackendService.Bases;
using ZKTeco.SyncBackendService.Models;

namespace ZKTeco.SyncBackendService.Connectors
{
    internal class ZKTecoConnector : ServiceBase
    {
        /// <summary>
        /// If failing to connect to the device, 
        /// it will reconnect several times.
        /// </summary>
        private int _retryTimes = 0;

        /// <summary>
        /// One ZKTeco device.
        /// </summary>
        private AxDeviceWrapper _device;

        private ManualResetEvent _signal;

        public ZKTecoConnector(AxDeviceWrapper device)
        {
            _device = device;
            _signal = new ManualResetEvent(false);
            Logger = HostLogger.Get<ZKTecoConnector>();
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Logger.Info("ZKTecoConnector.Start method runs.");
                if (Connect())
                {
                    Logger.Info("ZKTecoConnector.Connect method is ok.");
                    RegisterEvents();
                    Logger.Info("ZKTecoConnector.RegisterEvents method is ok.");
                }

                _signal.WaitOne();
                Logger.Info("ZKTecoConnector.Start method ends.");
            });           
        }

        public void Stop()
        {
            Logger.Info("ZKTecoConnector.Stop starts...");
            _signal.Set();
            _device.Disconnect();
            UnregisterEvents();
            Logger.Info("ZKTecoConnector.Stop ends.");
        }

        private bool Connect()
        {
            while (!_device.Connnect())
            {
                if (_retryTimes > ZKTecoConfig.RetryTimes)
                {
                    int errorCode = 0;
                    _device.Device.GetLastError(ref errorCode);
                    throw new System.Exception(string.Format("Unable to connect the device({0}:{1}), ErrorCode({2})",
                        _device.IP, _device.Port, errorCode));
                }

                Thread.Sleep(5000);
                _retryTimes++;
                Logger.ErrorFormat("Fail to connect to the device {Times} times.", _retryTimes);
            }

            return true;
        }

        public void RegisterEvents()
        {
            if (_device.RegisterAllEvents())
            {
                _device.Device.OnAttTransactionEx += OnAttTransactionEx;
                _device.Device.OnFinger += OnFinger;
                _device.Device.OnNewUser += OnNewUser;
                _device.Device.OnEnrollFingerEx += OnEnrollFingerEx;
                _device.Device.OnVerify += OnVerify;
                _device.Device.OnFingerFeature += OnFingerFeature;
                _device.Device.OnDoor += OnDoor;
                _device.Device.OnAlarm += OnAlarm;
                _device.Device.OnHIDNum += OnHIDNum;
                _device.Device.OnWriteCard += OnWriteCard;
                _device.Device.OnEmptyCard += OnEmptyCard;
                //_device.Device.OnDeleteTemplate += OnDeleteTemplate;

                Logger.Info("StartRealTimeLogs");
                _device.StartRealTimeLogs();
            }
        }

        public void UnregisterEvents()
        {          
            _device.Device.OnAttTransactionEx -= OnAttTransactionEx;
            _device.Device.OnFinger -= OnFinger;
            _device.Device.OnNewUser -= OnNewUser;
            _device.Device.OnEnrollFingerEx += OnEnrollFingerEx;
            _device.Device.OnVerify -= OnVerify;
            _device.Device.OnFingerFeature -= OnFingerFeature;
            _device.Device.OnDoor -= OnDoor;
            _device.Device.OnAlarm -= OnAlarm;
            _device.Device.OnHIDNum -= OnHIDNum;
            _device.Device.OnWriteCard -= OnWriteCard;
            _device.Device.OnEmptyCard -= OnEmptyCard;
            //_device.Device.OnDeleteTemplate -= OnDeleteTemplate;
        }

        /// <summary>
        /// If your fingerprint(or your card) passes the verification,this event will be triggered
        /// </summary>
        /// <param name="enrollNumber">UserID of a user</param>
        /// <param name="isInValid">Whether a record is valid. 1: Not valid. 0: Valid.</param>
        /// <param name="attState"></param>
        /// <param name="verifyMethod"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="workCode">
        /// work code returned during verification. 
        /// Return 0 when the device does not support work code.
        /// </param>
        private void OnAttTransactionEx(string enrollNumber, int isInValid, int attState, int verifyMethod, 
            int year, int month, int day, int hour, int minute, int second, int workCode)
        {
            //if (isInValid > 0)
            //{
            //    // The current user doesn't pass the verification.
            //    return;
            //}
            Logger.InfoFormat("OnAttTransactionEx:[@AttendanceLog], IsInValid[{IsInValid}].", 
                new AttendanceLog(enrollNumber, (AttendanceState)attState, (VerificationMode)verifyMethod, year, month, day, hour, minute, second, workCode),
                isInValid);
        }

        /// <summary>
        /// This event is triggered when a fingerprint is placed on the fingerprint sensor of the device.
        /// </summary>
        private void OnFinger()
        {
            Logger.Info("OnFinger.");
        }

        /// <summary>
        /// This event is triggered when a new user is successfully enrolled.
        /// </summary>
        /// <param name="enrollNumber">UserID of the newly enrolled user.</param>
        private void OnNewUser(int enrollNumber)
        {
            Logger.InfoFormat("OnNewUser:EnrollNumber[{EnrollNumber}].", enrollNumber);
        }

        /// <summary>
        /// This event is triggered when a fingerprint is registered
        /// </summary>
        /// <param name="enrollNumber">User ID of the fingerprint being registered</param>
        /// <param name="fingerIndex">Index of the current fingerprint</param>
        /// <param name="actionResult">Operation result. Return 0 if the operation is successful, or return a value greater than 0.</param>
        /// <param name="templateLength">Length of the fingerprint template</param>
        private void OnEnrollFingerEx(string enrollNumber, int fingerIndex, int actionResult, int templateLength)
        {
            Logger.InfoFormat("OnEnrollFingerEx:EnrollNumber[{EnrollNumber}], FingerIndex[{FingerIndex}], ActionResult[{ActionResult}], TemplateLength[{TemplateLength}].",
                enrollNumber, fingerIndex, actionResult, templateLength);
        }

        /// <summary>
        /// This event is triggered when a user is verified.
        /// </summary>
        /// <param name="userID">When verification succeeds, UserID indicates the ID of the user. Return the card number if card verification is successful, or return -1.</param>
        private void OnVerify(int userID)
        {
            Logger.InfoFormat("OnVerify:UserID[{UserID}].", userID);
        }

        /// <summary>
        /// This event is triggered when a user places a finger and the device registers the fingerprint.
        /// </summary>
        /// <param name="score"></param>
        private void OnFingerFeature(int score)
        {
            Logger.InfoFormat("OnFingerFeature:Score[{Score}]", score);
        }

        /// <summary>
        /// This event is triggered when the device opens the door.
        /// </summary>
        /// <param name="eventType">
        /// Open door type 
        /// 4: The door is not closed. 53: Exit button. 5: The door is closed. 1: The door is opened unexpectedly.
        /// </param>
        private void OnDoor(int eventType)
        {
            Logger.InfoFormat("OnDoor:EventType[{EventType}]", eventType);
        }

        /// <summary>
        /// This event is triggered when the device reports an alarm.
        /// </summary>
        /// <param name="alarmType">
        /// Type of an alarm. 
        /// 55: Tamper alarm. 58: False alarm. 32: Threatened alarm. 34: Anti-pass back alarm.
        /// </param>
        /// <param name="enrollNumber">
        /// User ID. 
        /// The value is 0 when a tamper alarm, false alarm, or threatened alarm is given.The value is the user ID when
        /// other threatened alarm or anti-pass back alarm is given.
        /// </param>
        /// <param name="verified">
        /// Whether to verify 
        /// The value is 0 when a tamper alarm, false alarm, or threatened alarm is given.The value is 1 when other alarms are given.
        /// </param>
        private void OnAlarm(int alarmType, int enrollNumber, int verified)
        {
            Logger.InfoFormat("OnAlarm:AlarmType[{AlarmType}],EnrollNumber[{EnrollNumber}],Verified[{Verified}]", alarmType, enrollNumber, verified);
        }

        /// <summary>
        /// This event is triggered when a card is swiped.
        /// </summary>
        /// <param name="cardNumber">
        /// Number of a card that can be an ID card or an HID card. If the card is a Mifare card, 
        /// the event is triggered only when the card is used as an ID card.
        /// </param>
        private void OnHIDNum(int cardNumber)
        {
            Logger.InfoFormat("OnHIDNum: CardNumber[{CardNumber}]", cardNumber);
        }

        /// <summary>
        /// This event is triggered when the device writes a card.
        /// </summary>
        /// <param name="enrollNumber">ID of the user to be written into a card</param>
        /// <param name="actionResult">Result of writing user information into a card. 0: Success. Other values:Failure.</param>
        /// <param name="length">Size of the data to be written into a card</param>
        private void OnWriteCard(int enrollNumber, int actionResult, int length)
        {
            Logger.InfoFormat("OnWriteCard: EnrollNumber[{EnrollNumber}], ActionResult[{ActionResult}], Length[{Length}].", enrollNumber);
        }

        /// <summary>
        /// This event is triggered when a Mifare card is emptied.
        /// </summary>
        /// <param name="actionResult">
        /// Result of emptying a card. 0: Success. Other values: Failure.
        /// </param>
        private void OnEmptyCard(int actionResult)
        {
            Logger.InfoFormat("OnEmptyCard: ActionResult[{ActionResult}]", actionResult);
        }

        /// <summary>
        /// When you have deleted one one fingerprint template,this event will be triggered.
        /// </summary>
        /// <param name="enrollNumber">User ID of the fingerprint being registered</param>
        /// <param name="fingerIndex">Index of the current fingerprint</param>
        private void OnDeleteTemplate(int enrollNumber, int fingerIndex)
        {
            Logger.InfoFormat("OnDeleteTemplate:EnrollNumber[{EnrollNumber}], FingerIndex[{FingerIndex}].", enrollNumber, fingerIndex);
        }
    }
}
