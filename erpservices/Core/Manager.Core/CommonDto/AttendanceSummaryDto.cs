using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Manager.Core.CommonDto
{
    public class AttendanceSummaryDto
    {
        public int AttendanceStatusID { get; set; }
        public string AttendanceStatus { get; set; }
        public int TotalCount { get; set; }
        public string Title { get; set; }
        public string TitleID { get; set; }
        public string BackgroundColor {
            get
            {

                string status = "";

                switch (AttendanceStatusID)
                {
                    case (int)Util.AttendanceStatus.Absent:
                        status = "#B71C1C";
                        break;
                    case (int)Util.AttendanceStatus.Late:
                        status = "#EF5350";
                        break;
                    case (int)Util.AttendanceStatus.Holiday:
                    case (int)Util.AttendanceStatus.OffDay:
                    case (int)Util.AttendanceStatus.Weekend:
                        status = "#2196F3";
                        break;
                    case (int)Util.AttendanceStatus.Invalid:
                        status = "#FF9800";
                        break;
                    case (int)Util.AttendanceStatus.Leave:
                        status = "#9C27B0";
                        break;
                    default:
                        status = "#4CAF50";
                        break;
                }
                return status;
            }
        }
        public string HoverBackgroundColor
        {
            get
            {

                string status = "";

                switch (AttendanceStatusID)
                {
                    case (int)Util.AttendanceStatus.Absent:
                        status = "#bd5f5f";
                        break;
                    case (int)Util.AttendanceStatus.Late:
                        status = "#f39b9a";
                        break;
                    case (int)Util.AttendanceStatus.Holiday:
                    case (int)Util.AttendanceStatus.OffDay:
                    case (int)Util.AttendanceStatus.Weekend:
                        status = "#7ea7c7";
                        break;
                    case (int)Util.AttendanceStatus.Invalid:
                        status = "#ecae52";
                        break;
                    case (int)Util.AttendanceStatus.Leave:
                        status = "#ab71b5";
                        break;
                    default:
                        status = "#89b78b";
                        break;
                }
                return status;
            }
        }
    }
}
