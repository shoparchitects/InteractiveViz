using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shop
{

   [Serializable]
   public class AirTable_ResponseClasses
   {
       public Record[] records;
   }

   [Serializable]
   public class Record
    {
       public string id;
       public Project fields;
       public string createdTime;
   }

    [Serializable]
    public class Project
    {
        public string Client;
        public string ClientEmailEndingForVerification;

        public string[] ApprovedProjectUserList;

        //Fields for ApprovedProjectUserList Table
        public string Email;
        public string[] ApprovedProjects; //this in actuality is a list of project IDs by default in airtable, not the names of projects
        
        public string PrefabInsideAssetBundleName;
        public bool HasConstructionData;
        public bool HasProgramBlocking;
        public string ConstructionDataUrl;
        public string BundleDirectDownloadUrl_iOS;
        public string BundleDirectDownloadUrl_OSX;
        public string BundleDirectDownloadUrl_Win;
        public string BundleDirectDownloadUrl_And;
        public string ProjectName;
        public string ProjectAddress;
        public float ShadowDistance;
        public uint AssetBundleVersion;
        public string FeaturedProject;
        //public string ProgramBlockingMaps;

        public string GuestCodeList;

        public string OrbitCameraTransformPosition;
        public string SectionCutBoxColliderExtents;
        private bool OrbitCameraTransformPositionSet = false;
        private bool SectionCutBoxColliderExtentsSet = false;
        public Vector3 _SectionCutBoxColliderExtents;
        public Vector3 _OrbitCameraTransformPosition;
        public Vector3 _OrbitCameraTransformRotation;

        public Vector3 OrbitCameraTransformPositionVector
        {
            get
            {
                if (_OrbitCameraTransformPosition == null || !OrbitCameraTransformPositionSet)
                {
                    _OrbitCameraTransformPosition = SplitAirtableVector3(OrbitCameraTransformPosition);
                    OrbitCameraTransformPositionSet = true;
                }
                return _OrbitCameraTransformPosition;
            }
        }
        public string OrbitCameraTransformRotation;
        private bool OrbitCameraTransformRotationSet = false;
        public Vector3 OrbitCameraTransformRotationVector
        {
            get
            {
                if (_OrbitCameraTransformRotation == null || !OrbitCameraTransformRotationSet)
                {
                    _OrbitCameraTransformRotation = SplitAirtableVector3(OrbitCameraTransformRotation);
                    OrbitCameraTransformRotationSet = true;
                }
                return _OrbitCameraTransformRotation;
            }
        }

        public Vector3 SectionCutBoxColliderExtentsVector
        {
            get
            {
                if (_SectionCutBoxColliderExtents == null || !SectionCutBoxColliderExtentsSet)
                {
                    _SectionCutBoxColliderExtents = SplitAirtableVector3(SectionCutBoxColliderExtents);
                    SectionCutBoxColliderExtentsSet = true;
                }
                return _SectionCutBoxColliderExtents;
            }
}
        

        public float Version;
        public string VersionType;
        // should be a bool but doesn't show in json if false.. ask Tim
        public bool DebugMode;
        public string[] Projects;

        // TO DO: airtable nested array not working.. break up string at commas.. find better solution
        public string[] RenderingUrls;

        public Vector3 SplitAirtableVector3(string airtableVector3)
        {
            string[] splitArray = airtableVector3.Split(char.Parse(","));

            var x = float.Parse(splitArray[0]);
            var y = float.Parse(splitArray[1]);
            var z = float.Parse(splitArray[2]);

            Vector3 vec = new Vector3(x, y, z);

            return vec;
        }
    }

    //=============== CONSTRUCTION STATUS ===============//

    [Serializable]
    public class AirTable_ResponseClasses_ConstructionData
    {
        public AirTable_Record_ConstructionData[] records;
        public string offset;
    }

    [Serializable]
    public class AirTable_Record_ConstructionData
    {
        public string id;
        public Fields_ConstructionData fields;
        public string createdTime;
    }

    [Serializable]
    public class Fields_ConstructionData
    {
        //TowerSlabs BlockSlabs FloorPlates Foundation Structure - Detroit
        //Slab Glazing - 9 Dekalb

        public string Floor;
        public string Section;
        public string Element;
        public string Progress;
        public string Date;
        public string ShowDate;
        public string ShowUnbuilt;
        public string Key;
        public string Height;
        
    }

    //=============== PROGRAM BLOCKING ===============//

    [Serializable]
    public class AirTable_ResponseClasses_ProgramBlocking
    {
        public AirTable_Record_ProgramBlocking[] records;
        public string offset;
    }

    [Serializable]
    public class AirTable_Record_ProgramBlocking
    {
        public string id;
        public AirTable_Fields_ProgramBlocking fields;
        public string createdTime;
    }

    [Serializable]
    public class AirTable_Fields_ProgramBlocking
    {
        //ProgramBlockingMaps AND ProjectPortalCopied shared fields
        public string ProjectName;
        public string[] ProjectIDs;

        //ProgramBlockingMaps fields
        public string ProjectSpecificProgramDisplayName;

        //GlobalProgramColorMap fields
        public string[] GlobalPrograms;
        public string RGB;

        //ProgramBlockingMaps AND GlobalProgramColorMap shared fields
        public string GlobalProgramName;
        public string[] GlobalProgramIDs;
        public string Color;


        public bool GetColor(out Color result)
        {
            result = new Color();
            float r = 0, g = 0, b = 0;
            bool success = false;

            if (!string.IsNullOrEmpty(RGB))
            {
                string[] rgbValues = RGB.Split(',');
                
                if (rgbValues.Length == 3)
                {
                    success = float.TryParse(rgbValues[0].Trim(), out r)
                           && float.TryParse(rgbValues[1].Trim(), out g)
                           && float.TryParse(rgbValues[2].Trim(), out b);
                }
                if(success)
                {
                    result.r = r / 255;
                    result.g = g / 255;
                    result.b = b / 255;
                }
            }
            return success;
        }
    }
}