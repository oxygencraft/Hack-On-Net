using HackLinks_Server.Files;
using System;

namespace HackLinks_Server.Computers
{
    class FilePermissions {

        public FilePermissions(File file)
        {
            this.file = file;
        }

        public enum PermissionType
        {
            User = 100,
            Group = 10,
            Others = 1,
        }

        public enum Permission
        {
            None = 0,
            Execute = 1,
            Write = 2,
            Read = 4,
        }

        private File file;

        private int permissionValue = 0;
        public int PermissionValue { get => permissionValue; set { permissionValue = value; file.Dirty = true; } }

        public int Owner { get => GetPermissionDigit(PermissionType.User); set => SetPermission(PermissionType.User, value); }
        public int Group { get => GetPermissionDigit(PermissionType.Group); set => SetPermission(PermissionType.Group, value); }
        public int Others { get => GetPermissionDigit(PermissionType.Others); set => SetPermission(PermissionType.Others, value); }

        public static int CalculatePermissionDigit(bool read, bool write, bool execute)
        {
            int value = 0;
            if (execute)
                value |= (int)Permission.Execute;
            if (write)
                value |= (int)Permission.Write;
            if (read)
                value |= (int)Permission.Read;
            return value;
        }

        public void SetPermission(PermissionType type, bool read, bool write, bool execute)
        {
            SetPermission(type, CalculatePermissionDigit(read, write, execute));
        }

        public void SetPermission(PermissionType type, int value)
        {
            permissionValue = (PermissionValue % (int)type) + ((PermissionValue / ((int)type * 10)) * ((int)type * 10) + ((int)type * value));
        }

        public int GetPermissionDigit(PermissionType type)
        {
            return PermissionValue / (int)type % 10;
        }

        /// <summary>
        /// Check if the file has permission for the given operations for the given type.
        /// </summary>
        /// <param name="type">The <see cref="PermissionType"/> to check</param>
        /// <param name="read">Set true if you want to check read permission</param>
        /// <param name="write">Set true if you want to check write permission</param>
        /// <param name="execute">Set true if you want to check execute permission</param>
        /// <returns>True if the type would have permission to perform the operation, false otherwise</returns>
        public bool CheckPermission(PermissionType type, bool read, bool write, bool execute)
        {
            return CheckPermissionDigit(type, CalculatePermissionDigit(read, write, execute));
        }

        /// <summary>
        /// <para>Check if the file has permission for the given operations for the given type.</para>
        /// <para>To calculate the value do a bitwise OR for the value of the required <see cref="Permission"/></para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value">The permission as a digit calculated from <see cref="Permission"/>.</param>
        /// <returns>True if the type would have permission to perform the operation, false otherwise</returns>
        public bool CheckPermissionDigit(PermissionType type, int value)
        {
            return (GetPermissionDigit(type) & value) == value;
        }
    }
}
