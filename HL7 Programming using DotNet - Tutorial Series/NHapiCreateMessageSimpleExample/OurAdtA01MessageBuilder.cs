using System;
using System.Globalization;
using NHapi.Model.V23.Message;

namespace NHapiCreateMessageSimpleExample
{
    internal class OurAdtA01MessageBuilder
    {
        private ADT_A01 _adtMessage;

        /*You can pass in a domain or data transfer object as a parameter
        when integrating with data from your application here
        I will leave that to you to explore on your own
        Using fictional data here for illustration*/

        public ADT_A01 Build()
        {
            var currentDateTimeString = GetCurrentTimeStamp();
            _adtMessage = new ADT_A01();
            
            CreateMshSegment(currentDateTimeString);
            CreateEvnSegment(currentDateTimeString);
            CreatePidSegment();
            CreatePv1Segment();
            return _adtMessage;
        }

        private void CreateMshSegment(string currentDateTimeString) 
        {
            var mshSegment = _adtMessage.MSH;
            mshSegment.FieldSeparator.Value = "|";
            mshSegment.EncodingCharacters.Value = "^~\\&";
            mshSegment.SendingApplication.NamespaceID.Value = "Our System";
            mshSegment.SendingFacility.NamespaceID.Value = "Our Facility";
            mshSegment.ReceivingApplication.NamespaceID.Value = "Their Remote System";
            mshSegment.ReceivingFacility.NamespaceID.Value = "Their Remote Facility";
            mshSegment.DateTimeOfMessage.TimeOfAnEvent.Value = currentDateTimeString;
            mshSegment.MessageControlID.Value = GetSequenceNumber();
            mshSegment.MessageType.MessageType.Value = "ADT";
            mshSegment.MessageType.TriggerEvent.Value = "A01";
            mshSegment.VersionID.Value = "2.3";
            mshSegment.ProcessingID.ProcessingID.Value = "P";
        }

        private void CreateEvnSegment(string currentDateTimeString) 
        {
            var evn = _adtMessage.EVN;
            evn.EventTypeCode.Value = "A01";
            evn.RecordedDateTime.TimeOfAnEvent.Value = currentDateTimeString;
        }

        private void CreatePidSegment() 
        {
            var pid = _adtMessage.PID;
            var patientName = pid.GetPatientName(0);
            patientName.FamilyName.Value = "Mouse";
            patientName.GivenName.Value = "Mickey";
            pid.SetIDPatientID.Value = "378785433211";
            var patientAddress = pid.GetPatientAddress(0);
            patientAddress.StreetAddress.Value = "123 Main Street";
            patientAddress.City.Value = "Lake Buena Vista";
            patientAddress.StateOrProvince.Value = "FL";
            patientAddress.Country.Value = "USA";
        }

        private void CreatePv1Segment() 
        {
            var pv1 = _adtMessage.PV1;
            pv1.PatientClass.Value = "O"; // to represent an 'Outpatient'
            var assignedPatientLocation = pv1.AssignedPatientLocation;
            assignedPatientLocation.Facility.NamespaceID.Value = "Some Treatment Facility";
            assignedPatientLocation.PointOfCare.Value = "Some Point of Care";
            pv1.AdmissionType.Value = "ALERT";
            var referringDoctor = pv1.GetReferringDoctor(0);
            referringDoctor.IDNumber.Value = "99999999";
            referringDoctor.FamilyName.Value = "Smith";
            referringDoctor.GivenName.Value = "Jack";
            referringDoctor.IdentifierTypeCode.Value = "456789";
            pv1.AdmitDateTime.TimeOfAnEvent.Value = GetCurrentTimeStamp();
        }

        private static string GetCurrentTimeStamp()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss",CultureInfo.InvariantCulture);
        }

        private static string GetSequenceNumber()
        {
            const string facilityNumberPrefix = "1234"; // some arbitrary prefix for the facility
            return facilityNumberPrefix + GetCurrentTimeStamp();
        }
    }
}