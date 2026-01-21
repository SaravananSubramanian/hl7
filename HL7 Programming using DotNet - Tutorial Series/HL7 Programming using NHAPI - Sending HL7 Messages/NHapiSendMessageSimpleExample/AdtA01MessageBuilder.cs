using System.Globalization;
using NHapi.Model.V23.Message;

namespace Com.SaravananSubramanian.Nhapi.SendMessages;

/// <summary>
/// Builder class for constructing ADT A01 (Admit/Visit Notification) messages.
/// </summary>
internal class AdtA01MessageBuilder
{
    private ADT_A01? _adtMessage;

    public ADT_A01 Build()
    {
        var timestamp = GetCurrentTimestamp();
        _adtMessage = new ADT_A01();

        BuildMshSegment(timestamp);
        BuildEvnSegment(timestamp);
        BuildPidSegment();
        BuildPv1Segment();

        return _adtMessage;
    }

    private void BuildMshSegment(string timestamp)
    {
        var msh = _adtMessage!.MSH;
        msh.FieldSeparator.Value = "|";
        msh.EncodingCharacters.Value = "^~\\&";
        msh.SendingApplication.NamespaceID.Value = "Our System";
        msh.SendingFacility.NamespaceID.Value = "Our Facility";
        msh.ReceivingApplication.NamespaceID.Value = "Their Remote System";
        msh.ReceivingFacility.NamespaceID.Value = "Their Remote Facility";
        msh.DateTimeOfMessage.TimeOfAnEvent.Value = timestamp;
        msh.MessageControlID.Value = GenerateMessageControlId();
        msh.MessageType.MessageType.Value = "ADT";
        msh.MessageType.TriggerEvent.Value = "A01";
        msh.VersionID.Value = "2.3";
        msh.ProcessingID.ProcessingID.Value = "P";
    }

    private void BuildEvnSegment(string timestamp)
    {
        var evn = _adtMessage!.EVN;
        evn.EventTypeCode.Value = "A01";
        evn.RecordedDateTime.TimeOfAnEvent.Value = timestamp;
    }

    private void BuildPidSegment()
    {
        var pid = _adtMessage!.PID;
        pid.SetIDPatientID.Value = "1";

        var patientName = pid.GetPatientName(0);
        patientName.FamilyName.Value = "Mouse";
        patientName.GivenName.Value = "Mickey";

        var patientAddress = pid.GetPatientAddress(0);
        patientAddress.StreetAddress.Value = "123 Main Street";
        patientAddress.City.Value = "Lake Buena Vista";
        patientAddress.StateOrProvince.Value = "FL";
        patientAddress.Country.Value = "USA";
    }

    private void BuildPv1Segment()
    {
        var pv1 = _adtMessage!.PV1;
        pv1.PatientClass.Value = "O";

        var location = pv1.AssignedPatientLocation;
        location.Facility.NamespaceID.Value = "Some Treatment Facility";
        location.PointOfCare.Value = "ER";

        pv1.AdmissionType.Value = "E";

        var referringDoctor = pv1.GetReferringDoctor(0);
        referringDoctor.IDNumber.Value = "99999999";
        referringDoctor.FamilyName.Value = "Smith";
        referringDoctor.GivenName.Value = "Jack";
        referringDoctor.IdentifierTypeCode.Value = "NPI";

        pv1.AdmitDateTime.TimeOfAnEvent.Value = GetCurrentTimestamp();
    }

    private static string GetCurrentTimestamp() =>
        DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

    private static string GenerateMessageControlId() =>
        $"FAC001{GetCurrentTimestamp()}{Random.Shared.Next(1000, 9999)}";
}
