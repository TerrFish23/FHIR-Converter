﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests.FilterTests
{
    public class SegmentFiltersTests
    {
        private const string TestData = @"MSH|^~\&|NISTEHRAPP|NISTEHRFAC|NISTIISAPP|NISTIISFAC|20150624084727.655-0500||VXU^V04^VXU_V04|NIST-IZ-AD-2.1_Send_V04_Z22|P|2.5.1|||ER|AL|||||Z22^CDCPHINVS|NISTEHRFAC|NISTIISFAC
PID|1||90012^^^NIST-MPI-1^MR||Wong^Elise^^^^^L||19830615|F||2028-9^Asian^CDCREC|9200 Wellington Trail^^Bozeman^MT^59715^USA^P||^PRN^PH^^^406^5557896~^NET^^Elise.Wong@isp.com|||||||||2186-5^Not Hispanic or Latino^CDCREC||N|1|||||N
PD1|||||||||||02^Reminder/recall - any method^HL70215|N|20150624|||A|19830615|20150624
ORC|RE|4422^NIST-AA-IZ-2|13696^NIST-AA-IZ-2|||||||7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN||654^Thomas^Wilma^Elizabeth^^^^^NIST-PI-1^L^^^MD|||||NISTEHRFAC^NISTEHRFacility^HL70362|
RXA|0|1|20150624||49281-0215-88^TENIVAC^NDC|0.5|mL^mL^UCUM||00^New Record^NIP001|7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN|^^^NIST-Clinic-1||||315841|20151216|PMC^Sanofi Pasteur^MVX|||CP|A
RXR|C28161^Intramuscular^NCIT|RD^Right Deltoid^HL70163
OBX|1|CE|30963-3^Vaccine Funding Source^LN|1|PHC70^Private^CDCPHINVS||||||F|||20150624
OBX|2|CE|64994-7^Vaccine Funding Program Eligibility^LN|2|V01^Not VFC Eligible^HL70064||||||F|||20150624|||VXC40^per immunization^CDCPHINVS
OBX|3|CE|69764-9^Document Type^LN|3|253088698300028811170411^Tetanus/Diphtheria (Td) Vaccine VIS^cdcgs1vis||||||F|||20150624
OBX|4|DT|29769-7^Date Vis Presented^LN|3|20150624||||||F|||20150624
ORC|RE||38760^NIST-AA-IZ-2|||||||7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN|||||||NISTEHRFAC^NISTEHRFacility^HL70362
RXA|0|1|20141012||88^influenza, unspecified formulation^CVX|999|||01^Historical Administration^NIP001|||||||||||CP|A
ORC|RE||35508^NIST-AA-IZ-2|||||||7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN|||||||NISTEHRFAC^NISTEHRFacility^HL70362
RXA|0|1|20131112||88^influenza, unspecified formulation^CVX|999|||01^Historical Administration^NIP001|||||||||||CP|A";

        [Fact]
        public void GivenAnHl7v2Data_WhenGetFirstSegments_CorrectResultShouldBeReturned()
        {
            Assert.Empty(Filters.GetFirstSegments(new Hl7v2Data(), "PID"));

            var data = LoadTestData();
            Assert.Empty(Filters.GetFirstSegments(data, string.Empty));

            var segments = Filters.GetFirstSegments(data, "PID|PD1|PV1|ORC");
            Assert.Equal(@"PID|1||90012^^^NIST-MPI-1^MR||Wong^Elise^^^^^L||19830615|F||2028-9^Asian^CDCREC|9200 Wellington Trail^^Bozeman^MT^59715^USA^P||^PRN^PH^^^406^5557896~^NET^^Elise.Wong@isp.com|||||||||2186-5^Not Hispanic or Latino^CDCREC||N|1|||||N", segments["PID"].Value);
            Assert.Equal(@"PD1|||||||||||02^Reminder/recall - any method^HL70215|N|20150624|||A|19830615|20150624", segments["PD1"].Value);
            Assert.Equal(@"ORC|RE|4422^NIST-AA-IZ-2|13696^NIST-AA-IZ-2|||||||7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN||654^Thomas^Wilma^Elizabeth^^^^^NIST-PI-1^L^^^MD|||||NISTEHRFAC^NISTEHRFacility^HL70362|", segments["ORC"].Value);
            Assert.True(!segments.ContainsKey("PV1"));

            Assert.Throws<NullReferenceException>(() => Filters.GetFirstSegments(null, "PID"));
            Assert.Throws<NullReferenceException>(() => Filters.GetFirstSegments(new Hl7v2Data(), null));
        }

        [Fact]
        public void GivenAnHl7v2Data_WhenGetSegmentLists_CorrectResultShouldBeReturned()
        {
            Assert.Empty(Filters.GetSegmentLists(new Hl7v2Data(), "PID"));

            var data = LoadTestData();
            Assert.Empty(Filters.GetSegmentLists(data, string.Empty));

            var segments = Filters.GetSegmentLists(data, "PID|PV1|ORC|OBX");
            Assert.Single(segments["PID"]);
            Assert.Equal(3, segments["ORC"].Count);
            Assert.Equal(4, segments["OBX"].Count);
            Assert.True(!segments.ContainsKey("PV1"));

            Assert.Throws<NullReferenceException>(() => Filters.GetSegmentLists(null, "PID"));
            Assert.Throws<NullReferenceException>(() => Filters.GetSegmentLists(new Hl7v2Data(), null));
        }

        [Fact]
        public void GivenAnHl7v2Data_WhenGetRelatedSegmentList_CorrectResultShouldBeReturned()
        {
            Assert.Empty(Filters.GetRelatedSegmentList(new Hl7v2Data(), null, null));

            var data = LoadTestData();
            var firstSegments = Filters.GetFirstSegments(data, "ORC");
            var orcSegment = firstSegments["ORC"];

            var rxaSegments = Filters.GetRelatedSegmentList(data, orcSegment, "RXA")["RXA"];
            Assert.Single(rxaSegments);

            var obxSegments = Filters.GetRelatedSegmentList(data, rxaSegments.First(), "OBX")["OBX"];
            Assert.Equal(4, obxSegments.Count);

            var pidSegments = Filters.GetRelatedSegmentList(data, orcSegment, "FOO");
            Assert.True(!pidSegments.ContainsKey("FOO"));

            Assert.Throws<NullReferenceException>(() => Filters.GetRelatedSegmentList(null, null, null));
        }

        [Fact]
        public void GivenAnHl7v2Data_WhenGetParentSegments_CorrectResultShouldBeReturned()
        {
            Assert.Empty(Filters.GetParentSegment(new Hl7v2Data(), "OBX", 3, "RXA"));

            var data = LoadTestData();
            var rxaSegment = Filters.GetParentSegment(data, "OBX", 3, "RXA")["RXA"];
            Assert.Equal(@"RXA|0|1|20150624||49281-0215-88^TENIVAC^NDC|0.5|mL^mL^UCUM||00^New Record^NIP001|7824^Jackson^Lily^Suzanne^^^^^NIST-PI-1^L^^^PRN|^^^NIST-Clinic-1||||315841|20151216|PMC^Sanofi Pasteur^MVX|||CP|A", rxaSegment.Value);

            Assert.Empty(Filters.GetParentSegment(data, "OBX", 4, "FOO"));

            Assert.Throws<NullReferenceException>(() => Filters.GetParentSegment(null, "OBX", 3, "RXA"));
        }

        [Fact]
        public void GivenAnHl7v2Data_WhenHasSegments_CorrectResultShouldBeReturned()
        {
            Assert.False(Filters.HasSegments(new Hl7v2Data(), "PID"));

            var data = LoadTestData();
            Assert.False(Filters.HasSegments(data, string.Empty));
            Assert.True(Filters.HasSegments(data, "PID"));
            Assert.True(Filters.HasSegments(data, "PID|ORC|OBX"));
            Assert.False(Filters.HasSegments(data, "PID|ORC|OBX||"));

            Assert.Throws<NullReferenceException>(() => Filters.HasSegments(null, "PID"));
            Assert.Throws<NullReferenceException>(() => Filters.HasSegments(new Hl7v2Data(), null));
        }

        private Hl7v2Data LoadTestData()
        {
            var parser = new Hl7v2DataParser();
            return parser.Parse(TestData);
        }
    }
}