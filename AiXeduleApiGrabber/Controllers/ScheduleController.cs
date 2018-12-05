using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AiXeduleApiGrabber.BackgroundTasks;
using AiXeduleApiGrabber.Models;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AiXeduleApiGrabber.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ScheduleController : Controller
    {
        public static string BaseUrl = "https://sa-nhlstenden.xedule.nl";
        private static Dictionary<string, CacheEntry> cache = new Dictionary<string, CacheEntry>();

        public IActionResult Index()
        {
            return NotFound();
        }
        /*
        public async Task<IActionResult> GetSchedule([FromQuery] string group, [FromQuery] int weekNr)
        {
            if (!string.IsNullOrWhiteSpace(group))
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Cookie",
                            $"User={SessionReviver.UserCookie}; ASP.NET_SessionId={SessionReviver.SessionCookie}");

                        string response = await client.GetStringAsync($"{BaseUrl}/api/group");
                        List<GroupEntry> groups = JsonConvert.DeserializeObject<List<GroupEntry>>(response);

                        GroupEntry groupEntry = groups.FirstOrDefault(g => g.Code.ToLowerInvariant() == group.ToLowerInvariant());
                        if (groupEntry == null)
                        {
                            return BadRequest();
                        }

                        response = await client.GetStringAsync($"{BaseUrl}/api/year");
                        List<YearEntry> years = JsonConvert.DeserializeObject<List<YearEntry>>(response);

                        response = await client.GetStringAsync($"{BaseUrl}/api/docent");
                        List<DocentEntry> docents = JsonConvert.DeserializeObject<List<DocentEntry>>(response);

                        response = await client.GetStringAsync($"{BaseUrl}/api/facility");
                        List<FacilityEntry> facilities = JsonConvert.DeserializeObject<List<FacilityEntry>>(response);

                        YearEntry yearEntry =
                            years.FirstOrDefault(y => y.Oru == groupEntry.Orus.First());

                        string[] scheduleIds = null;
                        if (weekNr <= 0 || weekNr > 52)
                        {
                            scheduleIds = GenerateScheduleFormats(yearEntry, groupEntry.Id);
                        }
                        else
                        {
                            scheduleIds = GenerateScheduleFormats(yearEntry, groupEntry.Id, new[] {weekNr});
                        }

                        response = await client.GetStringAsync(
                            $"{BaseUrl}/api/schedule/?{GenerateScheduleQuerry(scheduleIds)}");
                        List<ScheduleEntry> schedules = JsonConvert.DeserializeObject<List<ScheduleEntry>>(response);

                        return Ok(GenerateScheduleItems(schedules, docents, facilities));
                    }
                }
                catch (Exception e)
                {

                }
            }

            return NotFound();
        }
        */
        public async Task<IActionResult> GetIcalSchedule([FromQuery] string group, [FromQuery] string docent)
        {
            int weekNr = 200;
            if (!string.IsNullOrWhiteSpace(group) || !string.IsNullOrWhiteSpace(docent))
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Cookie",
                            $"User={SessionReviver.UserCookie}; ASP.NET_SessionId={SessionReviver.SessionCookie}");

                        List<GroupEntry> groups = new List<GroupEntry>();
                        List<DocentEntry> docents = new List<DocentEntry>();
                        string response;
                        ISubjectEntry subject = null;

                        if (!string.IsNullOrWhiteSpace(group))
                        {
                            response = await client.GetStringAsync($"{BaseUrl}/api/group");
                            groups = JsonConvert.DeserializeObject<List<GroupEntry>>(response);

                            GroupEntry groupEntry = groups.FirstOrDefault(g => g.Code.ToLowerInvariant() == group.ToLowerInvariant());
                            if (groupEntry == null)
                            {
                                return BadRequest();
                            }

                            if (cache.ContainsKey(groupEntry.Id))
                            {
                                if (DateTime.UtcNow < cache[groupEntry.Id].lastUpdated.AddHours(4))
                                {
                                    var serializer = new CalendarSerializer();
                                    string serializedString = serializer.SerializeToString(cache[groupEntry.Id].Calendar);
                                    return Ok(serializedString);
                                }
                            }

                            response = await client.GetStringAsync($"{BaseUrl}/api/docent");
                            docents = JsonConvert.DeserializeObject<List<DocentEntry>>(response);

                            subject = groupEntry;
                        } else if (!string.IsNullOrWhiteSpace(docent))
                        {
                            response = await client.GetStringAsync($"{BaseUrl}/api/docent");
                            docents = JsonConvert.DeserializeObject<List<DocentEntry>>(response);

                            DocentEntry docentEntry = docents.FirstOrDefault(d => d.Code.ToLowerInvariant() == docent.ToLowerInvariant());
                            if (docentEntry == null)
                            {
                                return BadRequest();
                            }

                            if (cache.ContainsKey(docentEntry.Id))
                            {
                                if (DateTime.UtcNow < cache[docentEntry.Id].lastUpdated.AddHours(4))
                                {
                                    var serializer = new CalendarSerializer();
                                    string serializedString = serializer.SerializeToString(cache[docentEntry.Id].Calendar);
                                    return Ok(serializedString);
                                }
                            }

                            response = await client.GetStringAsync($"{BaseUrl}/api/group");
                            groups = JsonConvert.DeserializeObject<List<GroupEntry>>(response);

                            subject = docentEntry;
                        }

                        response = await client.GetStringAsync($"{BaseUrl}/api/year");
                        List<YearEntry> years = JsonConvert.DeserializeObject<List<YearEntry>>(response);

                        response = await client.GetStringAsync($"{BaseUrl}/api/facility");
                        List<FacilityEntry> facilities = JsonConvert.DeserializeObject<List<FacilityEntry>>(response);

                        YearEntry yearEntry =
                            years.FirstOrDefault(y => y.Oru == subject.Orus.First());

                        string[] scheduleIds = null;
                        if (weekNr <= 0 || weekNr > 52)
                        {
                            scheduleIds = GenerateScheduleFormats(yearEntry, subject.Id);
                        }
                        else
                        {
                            scheduleIds = GenerateScheduleFormats(yearEntry, subject.Id, new[] { weekNr });
                        }

                        response = await client.GetStringAsync(
                            $"{BaseUrl}/api/schedule/?{GenerateScheduleQuerry(scheduleIds)}");
                        List<ScheduleEntry> schedules = JsonConvert.DeserializeObject<List<ScheduleEntry>>(response);

                        ScheduleResponse[] scheduleItems = GenerateScheduleItems(schedules, docents, facilities);

                        var icalCalendar = new Ical.Net.Calendar();
                        var calendarSerializer = new CalendarSerializer();

                        foreach (ScheduleResponse scheduleItem in scheduleItems)
                        {
                            StringBuilder descrBuilder = new StringBuilder();
                            if (scheduleItem.Teachers.Length > 0)
                            {
                                descrBuilder.AppendLine("Teachers:");
                                foreach (string teacher in scheduleItem.Teachers)
                                {
                                    descrBuilder.AppendLine(teacher);
                                }
                            }

                            if (scheduleItem.ClassRooms.Length > 0)
                            {
                                descrBuilder.AppendLine("\nClassrooms:");
                                foreach (string classroom in scheduleItem.ClassRooms)
                                {
                                    descrBuilder.AppendLine(classroom);
                                }
                            }

                            icalCalendar.Events.Add(new CalendarEvent()
                            {
                                Start = new CalDateTime(scheduleItem.StartTime),
                                End = new CalDateTime(scheduleItem.EndTime),
                                Summary = scheduleItem.Subject,
                                Description = descrBuilder.ToString()
                            });
                        }

                        icalCalendar.AddTimeZone("Europe/Amsterdam");
                        cache[subject.Id] = new CacheEntry()
                        {
                            GroupName = subject.Id,
                            Calendar = icalCalendar,
                            lastUpdated = DateTime.UtcNow
                        };
                        string icalString = calendarSerializer.SerializeToString(icalCalendar);
                        return Ok(icalString);
                    }
                }
                catch (Exception e)
                {

                }
            }

            return NotFound();
        }
        /*
        public async Task<IActionResult> GetCurrentSchedule([FromQuery] string group)
        {
            if (!string.IsNullOrWhiteSpace(group))
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Cookie",
                            $"User={SessionReviver.UserCookie}; ASP.NET_SessionId={SessionReviver.SessionCookie}");

                        string response = await client.GetStringAsync($"{BaseUrl}/api/group");
                        List<GroupEntry> groups = JsonConvert.DeserializeObject<List<GroupEntry>>(response);

                        GroupEntry groupEntry = groups.FirstOrDefault(g => g.Code.ToLowerInvariant() == group.ToLowerInvariant());
                        if (groupEntry == null)
                        {
                            return BadRequest();
                        }

                        response = await client.GetStringAsync($"{BaseUrl}/api/year");
                        List<YearEntry> years = JsonConvert.DeserializeObject<List<YearEntry>>(response);

                        response = await client.GetStringAsync($"{BaseUrl}/api/docent");
                        List<DocentEntry> docents = JsonConvert.DeserializeObject<List<DocentEntry>>(response);

                        response = await client.GetStringAsync($"{BaseUrl}/api/facility");
                        List<FacilityEntry> facilities = JsonConvert.DeserializeObject<List<FacilityEntry>>(response);

                        YearEntry yearEntry =
                            years.FirstOrDefault(y => y.Oru == groupEntry.Orus.First());

                        DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
                        Calendar cal = dfi.Calendar;

                        string [] scheduleIds = GenerateScheduleFormats(yearEntry, groupEntry.Id, new []{cal.GetWeekOfYear(DateTime.UtcNow, dfi.CalendarWeekRule, dfi.FirstDayOfWeek)});

                        response = await client.GetStringAsync(
                            $"{BaseUrl}/api/schedule/?{GenerateScheduleQuerry(scheduleIds)}");
                        List<ScheduleEntry> schedules = JsonConvert.DeserializeObject<List<ScheduleEntry>>(response);

                        return Ok(GenerateScheduleItems(schedules, docents, facilities));
                    }
                }
                catch (Exception e)
                {

                }
            }

            return NotFound();
        }
        */
        private string[] GenerateScheduleFormats(YearEntry oru, string groupId, int[] weekNrs = null)
        {
            List<string> scheduleIds = new List<string>();
            if (weekNrs == null)
            {
                weekNrs = oru.Schs;
            }
            foreach (int weekNr in weekNrs)
            {
                if (oru.Schs.Contains(weekNr))
                {
                    scheduleIds.Add($"{oru.Id}_{weekNr}_{groupId}");
                }
            }

            return scheduleIds.ToArray();
        }

        private string GenerateScheduleQuerry(string[] scheduleIds)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < scheduleIds.Length; i++)
            {
                stringBuilder.Append($"ids[{i}]={scheduleIds[i]}");
                if (i != scheduleIds.Length - 1)
                {
                    stringBuilder.Append("&");
                }
            }

            return stringBuilder.ToString();
        }

        private ScheduleResponse[] GenerateScheduleItems(List<ScheduleEntry> scheduleEntries, List<DocentEntry> docents,
            List<FacilityEntry> facilities)
        {
            List<CourseEntry> courseEntries = new List<CourseEntry>();
            List<ScheduleResponse> scheduleResponses = new List<ScheduleResponse>();

            foreach (ScheduleEntry scheduleEntry in scheduleEntries)
            {
                courseEntries.AddRange(scheduleEntry.Apps);
            }

            List <CourseEntry> sortedCourses = courseEntries.OrderBy(ce => ce.IStart).ToList();

            foreach (CourseEntry courseEntry in sortedCourses)
            {
                List<DocentEntry> courseDocents = new List<DocentEntry>();
                List<FacilityEntry> courseFacilities = new List<FacilityEntry>();
                if (courseEntry.Atts.Length <= 30)
                {
                    foreach (int docentId in courseEntry.Atts)
                    {
                        DocentEntry entry = docents.FirstOrDefault(d => d.Id == docentId.ToString());
                        if (entry != null)
                        {
                            courseDocents.Add(entry);
                        }
                    }

                    foreach (int facilityIds in courseEntry.Atts)
                    {
                        FacilityEntry entry = facilities.FirstOrDefault(d => d.Id == facilityIds.ToString());
                        if (entry != null)
                        {
                            courseFacilities.Add(entry);
                        }
                    }
                }

                scheduleResponses.Add(new ScheduleResponse()
                {
                    Subject = courseEntry.Name.Replace(":", "").Trim(),
                    StartTime = courseEntry.IStart,
                    EndTime = courseEntry.IEnd,
                    ClassRooms = courseFacilities.Select(f => f.Code).ToArray(),
                    Teachers = courseDocents.Select(d => d.Code).ToArray()
                });
            }

            return scheduleResponses.ToArray();
        }
    }
}