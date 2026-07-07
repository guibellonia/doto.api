using Doto.Application.DTOs.Requests.Schedule;
using Doto.Domain.Enums;
using Doto.Domain.Entities;

namespace Doto.Application.Helpers
{
    public class ScheduleHelper
    {
        public static void ApplyScheduleConfiguration(Schedule schedule, CreateScheduleRequest request)
        {
            switch (request.ScheduleType)
            {
                case MedicineScheduleType.OncePerDay:
                    schedule.SetOncePerDay(request.TimeOfDay!.Value);
                    break;

                case MedicineScheduleType.MultipleFixedTimesPerDay:
                    schedule.SetMultipleTimesPerDay(request.TimesOfDay!);
                    break;

                case MedicineScheduleType.EveryXHours:
                    schedule.SetEveryXHours(request.IntervalInHours!.Value, request.FirstDoseAt!.Value);
                    break;

                case MedicineScheduleType.SpecificWeekDays:
                    schedule.SetSpecificWeekDays(request.TimeOfDay!.Value, request.WeekDays!);
                    break;

                case MedicineScheduleType.AsNeeded:
                    schedule.SetAsNeeded();
                    break;
            }
        }

        public static void ApplyScheduleConfiguration(Schedule schedule, UpdateScheduleRequest request)
        {
            switch (request.ScheduleType)
            {
                case MedicineScheduleType.OncePerDay:
                    schedule.SetOncePerDay(request.TimeOfDay!.Value);
                    break;

                case MedicineScheduleType.MultipleFixedTimesPerDay:
                    schedule.SetMultipleTimesPerDay(request.TimesOfDay!);
                    break;

                case MedicineScheduleType.EveryXHours:
                    schedule.SetEveryXHours(request.IntervalInHours!.Value, request.FirstDoseAt!.Value);
                    break;

                case MedicineScheduleType.SpecificWeekDays:
                    schedule.SetSpecificWeekDays(request.TimeOfDay!.Value, request.WeekDays!);
                    break;

                case MedicineScheduleType.AsNeeded:
                    schedule.SetAsNeeded();
                    break;
            }
        }

        public static string? ValidateCreateRequest(CreateScheduleRequest request)
            => ValidateCommon(request.ScheduleType,
                request.TimeOfDay,
                request.TimesOfDay,
                request.IntervalInHours,
                request.FirstDoseAt,
                request.WeekDays);

        public static string? ValidateUpdateRequest(UpdateScheduleRequest request)
            => ValidateCommon(request.ScheduleType,
                request.TimeOfDay,
                request.TimesOfDay,
                request.IntervalInHours,
                request.FirstDoseAt,
                request.WeekDays);

        public static string? ValidateCommon(
            MedicineScheduleType scheduleType,
            TimeOnly? timeOfDay,
            List<TimeOnly>? timesOfDay,
            int? intervalInHours,
            DateTimeOffset? firstDoseAt,
            List<int>? weekDays)
        {
            switch (scheduleType)
            {
                case MedicineScheduleType.OncePerDay:
                    if (!timeOfDay.HasValue)
                        return "TimeOfDay is required for OncePerDay schedule.";
                    break;

                case MedicineScheduleType.MultipleFixedTimesPerDay:
                    if (timesOfDay == null || timesOfDay.Count == 0)
                        return "TimesOfDay is required for MultipleFixedTimesPerDay schedule.";
                    break;

                case MedicineScheduleType.EveryXHours:
                    if (!intervalInHours.HasValue || intervalInHours <= 0)
                        return "IntervalInHours must be greater than zero for EveryXHours schedule.";
                    if (!firstDoseAt.HasValue)
                        return "FirstDoseAt is required for EveryXHours schedule.";
                    break;

                case MedicineScheduleType.SpecificWeekDays:
                    if (!timeOfDay.HasValue)
                        return "TimeOfDay is required for SpecificWeekDays schedule.";
                    if (weekDays == null || weekDays.Count == 0)
                        return "WeekDays is required for SpecificWeekDays schedule.";
                    break;

                case MedicineScheduleType.AsNeeded:
                    break;
            }

            return null;
        }
    }
}
