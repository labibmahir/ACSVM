namespace Utilities.Constants
{
    public static class Enums
    {
        public enum Role : byte
        {
            SuperAdmin = 1,

            Admin = 2,

            User = 3
        }

        public enum Gender : byte
        {
            Male = 1,

            Female = 2,

            Other = 3
        }

        public enum FingerNumber : byte
        {
            LeftThumb = 1,

            LeftIndexFinger = 2,

            LeftMiddleFinger = 3,

            LeftRingFinger = 4,

            LeftLittleFinger = 5
            ,
            RightThumb = 6,

            RightIndexFinger = 7,

            RightMiddleFinger = 8,

            RightRingFinger = 9,

            RightLittleFinger = 10
        }

        public enum Status : byte
        {
            Active = 1,

            Inactive = 2,

            Allocated = 3,
            NotInService = 4
        }

        public enum PersonType : byte
        {
            NormalUser = 1,

            Visitor = 2,

            BlockedUser = 3
        }

        public enum UserVerifyMode : byte
        {
            faceAndFpAndCard = 1,

            faceOrFpOrCardOrPw = 2,

            card = 3
        }

        public enum DatabaseType
        {
            MySQL = 1,

            SQLServer = 2,

            Oracle = 3,

            PostgreSQL = 4,

            CustomSQLServer = 5
        }
    }
}
