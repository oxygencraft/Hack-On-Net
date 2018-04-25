using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Permissions
{
    static class PermissionHelper
    {
        /// <summary>
        /// Returns the <see cref="Group"/> for the given string or <see cref="Group.INVALID"/> if no match was found.
        /// </summary>
        /// <param name="groupString"></param>
        /// <returns>The matching Group or INVALID if no match</returns>
        public static Group GetGroupFromString(string groupString)
        {
            if (!Enum.TryParse(groupString.ToUpper(), out Group group) || !Enum.IsDefined(typeof(Group), group))
            {
                return Group.INVALID;
            }
            return group;
        }

        internal static bool ApplyModifiers(string modifer, FilePermissions permissions)
        {

            if (Regex.IsMatch(modifer, "^[0-7]{1,3}$"))
            {
                permissions.PermissionValue = int.Parse(modifer);
                return true;
            }

            Match match = Regex.Match(modifer, "^([augo]*)([+=-][rwx]*)+$");
            if(match.Success && match.Groups.Count >= 2)
            {

                string permissionTypeChars = match.Groups[1].Captures[0].Value;

                HashSet<FilePermissions.PermissionType> permissionTypes = new HashSet<FilePermissions.PermissionType>();

                int lengthOfTypes = Enum.GetValues(typeof(FilePermissions.PermissionType)).Length;

                if(permissionTypeChars.Length > 0)
                {
                    foreach (char permissionTypeChar in permissionTypeChars)
                    {
                        if (permissionTypes.Count > lengthOfTypes)
                        {
                            break;
                        }
                        switch (permissionTypeChar)
                        {
                            case 'u':
                                permissionTypes.Add(FilePermissions.PermissionType.User);
                                continue;
                            case 'g':
                                permissionTypes.Add(FilePermissions.PermissionType.Group);
                                continue;
                            case 'o':
                                permissionTypes.Add(FilePermissions.PermissionType.Others);
                                continue;
                            case 'a':
                                permissionTypes.UnionWith(Enum.GetValues(typeof(FilePermissions.PermissionType)).OfType<FilePermissions.PermissionType>());
                                continue;
                            default:
                                return false;
                        }
                    }
                }
                else
                {
                    //No type specified, default to all types
                    permissionTypes.UnionWith(Enum.GetValues(typeof(FilePermissions.PermissionType)).OfType<FilePermissions.PermissionType>());
                }


                foreach(Capture permissionCapture in match.Groups[2].Captures)
                {
                    string permission = permissionCapture.Value;
                    Console.WriteLine($"permission {permission}");
                    foreach (FilePermissions.PermissionType type in permissionTypes)
                    {
                        int permissionDigit = FilePermissions.CalculatePermissionDigit(permission.Contains('r'), permission.Contains('w'), permission.Contains('x'));
                        switch(permission[0])
                        {
                            case '+':
                                permissionDigit = permissions.GetPermissionDigit(type) | permissionDigit;
                                break;
                            case '=':
                                break;
                            case '-':
                                permissionDigit = permissions.GetPermissionDigit(type) ^ permissionDigit;
                                break;
                            // Logically there will never be anything but a '+', '=', or '-' here, so default throws exceptions in case of future bugs.
                            default:
                                throw new InvalidOperationException($"Invalid permission Modifier '{permission[0]}'");

                        }
                        permissions.SetPermission(type, permissionDigit);
                    }
                }
                return true;
            }

            return false;
        }

        public static string PermissionToDisplayString(FilePermissions permissions)
        {
            StringBuilder output = new StringBuilder();

            if(permissions.Owner != (int) FilePermissions.Permission.None)
            {
                if(output.Length > 0)
                {
                    output.Append(", ");
                }
                output.Append("U=");
                if ((permissions.Owner & (int)FilePermissions.Permission.Read) == (int)FilePermissions.Permission.Read) output.Append('R');
                else output.Append('-');
                if ((permissions.Owner & (int) FilePermissions.Permission.Write) == (int) FilePermissions.Permission.Write) output.Append('W');
                else output.Append('-');
                if ((permissions.Owner & (int) FilePermissions.Permission.Execute) == (int) FilePermissions.Permission.Execute) output.Append('X');
                else output.Append('-');
            }

            if (permissions.Group != (int)FilePermissions.Permission.None)
            {
                if (output.Length > 0)
                {
                    output.Append(", ");
                }
                output.Append("G=");
                if ((permissions.Group & (int)FilePermissions.Permission.Read) == (int)FilePermissions.Permission.Read) output.Append('R');
                else output.Append('-');
                if ((permissions.Group & (int)FilePermissions.Permission.Write) == (int)FilePermissions.Permission.Write) output.Append('W');
                else output.Append('-');
                if ((permissions.Group & (int)FilePermissions.Permission.Execute) == (int)FilePermissions.Permission.Execute) output.Append('X');
                else output.Append('-');
            }

            if (permissions.Others != (int)FilePermissions.Permission.None)
            {
                if (output.Length > 0)
                {
                    output.Append(", ");
                }
                output.Append("O=");
                if ((permissions.Others & (int)FilePermissions.Permission.Read) == (int)FilePermissions.Permission.Read) output.Append('R');
                else output.Append('-');
                if ((permissions.Others & (int)FilePermissions.Permission.Write) == (int)FilePermissions.Permission.Write) output.Append('W');
                else output.Append('-');
                if ((permissions.Others & (int)FilePermissions.Permission.Execute) == (int)FilePermissions.Permission.Execute) output.Append('X');
                else output.Append('-');
            }

            return output.ToString();
        }
    }
}
