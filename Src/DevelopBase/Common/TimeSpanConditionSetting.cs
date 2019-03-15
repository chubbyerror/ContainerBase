using System;
using System.Collections.Generic;
using System.Linq;

namespace DevelopBase.Common
{
    /// <summary>
    /// 时间条件
    /// </summary>
    public class TimeSpanConditionSetting
    {
        /// <summary>
        /// 天
        /// </summary>
        public int[] Day { get; set; }
        /// <summary>
        /// 小时
        /// </summary>
        public int[] Hour { get; set; }
        /// <summary>
        /// 分钟
        /// </summary>
        public int[] Minute { get; set; }
        /// <summary>
        /// 秒钟
        /// </summary>
        public int[] Second { get; set; }
        
        /// <summary>
        /// 时间条件
        /// </summary>
        public List<TimeSpan> TimeSpanConditions
        {
            get
            {
                List<TimeSpan> timeSpans = new List<TimeSpan>();
                timeSpans.AddRange(Day.Select(s => TimeSpan.FromDays(s)).ToList());
                timeSpans.AddRange(Hour.Select(s => TimeSpan.FromHours(s)).ToList());
                timeSpans.AddRange(Minute.Select(s => TimeSpan.FromMinutes(s)).ToList());
                timeSpans.AddRange(Second.Select(s => TimeSpan.FromSeconds(s)).ToList());
                //时间条件倒序排列
                timeSpans = timeSpans.OrderByDescending(o => o).ToList();
                return timeSpans;
            }
        }
    }
}
