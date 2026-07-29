using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "app_users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    display_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    password_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    weight_kg = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    height_cm = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: true),
                    avatar_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    time_zone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "America/Sao_Paulo"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_users", x => x.id);
                    table.CheckConstraint("ck_app_users_no_self_parent", "parent_id IS DISTINCT FROM id");
                    table.CheckConstraint("ck_app_users_role_parent_coherence", "(role = 'Child' AND parent_id IS NOT NULL) OR (role = 'Parent' AND parent_id IS NULL)");
                    table.ForeignKey(
                        name: "fk_app_users_app_users_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "child_invites",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    parent_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    invite_token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    temp_password_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    accepted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resent_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_sent_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_child_invites", x => x.id);
                    table.ForeignKey(
                        name: "fk_child_invites_app_users_child_user_id",
                        column: x => x.child_user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_child_invites_app_users_parent_user_id",
                        column: x => x.parent_user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    push_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    platform = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    device_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    app_version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    last_seen_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_devices", x => x.id);
                    table.ForeignKey(
                        name: "fk_devices_app_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "health_conditions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    diagnosed_on = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_health_conditions", x => x.id);
                    table.ForeignKey(
                        name: "fk_health_conditions_app_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medications",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    dosage_value = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    dosage_unit = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    observations = table.Column<string>(type: "text", nullable: true),
                    treatment_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    treatment_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    total_doses = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medications", x => x.id);
                    table.CheckConstraint("ck_medications_dosage_positive", "dosage_value > 0");
                    table.CheckConstraint("ck_medications_treatment_period", "treatment_end_date IS NULL OR treatment_end_date >= treatment_start_date");
                    table.ForeignKey(
                        name: "fk_medications_app_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_exports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    storage_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    generated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report_exports", x => x.id);
                    table.CheckConstraint("ck_report_exports_period", "period_end >= period_start");
                    table.ForeignKey(
                        name: "fk_report_exports_app_users_requested_by_user_id",
                        column: x => x.requested_by_user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_report_exports_app_users_subject_user_id",
                        column: x => x.subject_user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "symptom_records",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    severity = table.Column<short>(type: "smallint", nullable: true),
                    trend = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    recorded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    recorded_local_date = table.Column<DateOnly>(type: "date", nullable: false),
                    recorded_local_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_symptom_records", x => x.id);
                    table.CheckConstraint("ck_symptom_records_duration", "duration_minutes IS NULL OR duration_minutes >= 0");
                    table.CheckConstraint("ck_symptom_records_severity_range", "severity IS NULL OR severity BETWEEN 0 AND 10");
                    table.ForeignKey(
                        name: "fk_symptom_records_app_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vital_readings",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    value_primary = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    value_secondary = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    unit = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    context = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    measured_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    measured_local_date = table.Column<DateOnly>(type: "date", nullable: false),
                    measured_local_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vital_readings", x => x.id);
                    table.CheckConstraint("ck_vital_readings_blood_pressure_requires_diastolic", "type <> 'BloodPressure' OR value_secondary IS NOT NULL");
                    table.CheckConstraint("ck_vital_readings_secondary_only_blood_pressure", "type = 'BloodPressure' OR value_secondary IS NULL");
                    table.CheckConstraint("ck_vital_readings_value_primary_positive", "value_primary > 0");
                    table.ForeignKey(
                        name: "fk_vital_readings_app_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medication_schedules",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    medication_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recurrence_type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    days_of_week = table.Column<int>(type: "integer", nullable: false),
                    day_of_month = table.Column<short>(type: "smallint", nullable: true),
                    anchor_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    interval_minutes = table.Column<short>(type: "smallint", nullable: true),
                    min_interval_minutes = table.Column<short>(type: "smallint", nullable: true),
                    doses_per_day = table.Column<short>(type: "smallint", nullable: false),
                    late_grace_minutes = table.Column<short>(type: "smallint", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    generation_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    generated_through_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medication_schedules", x => x.id);
                    table.CheckConstraint("ck_medication_schedules_day_of_month", "(recurrence_type = 'Monthly' AND day_of_month BETWEEN 1 AND 31) OR (recurrence_type <> 'Monthly' AND day_of_month IS NULL)");
                    table.CheckConstraint("ck_medication_schedules_days_of_week_range", "days_of_week BETWEEN 0 AND 127");
                    table.CheckConstraint("ck_medication_schedules_doses_per_day", "doses_per_day >= 1");
                    table.CheckConstraint("ck_medication_schedules_interval_coherence", "interval_minutes IS NULL OR (anchor_time IS NOT NULL AND doses_per_day >= 1 AND interval_minutes > 0)");
                    table.CheckConstraint("ck_medication_schedules_late_grace", "late_grace_minutes >= 0");
                    table.CheckConstraint("ck_medication_schedules_period", "end_date IS NULL OR end_date >= start_date");
                    table.CheckConstraint("ck_medication_schedules_weekdays_required", "recurrence_type <> 'SpecificWeekDays' OR days_of_week > 0");
                    table.CheckConstraint("ck_medication_schedules_weekly_single_day", "recurrence_type <> 'Weekly' OR (days_of_week > 0 AND (days_of_week & (days_of_week - 1)) = 0)");
                    table.ForeignKey(
                        name: "fk_medication_schedules_app_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_medication_schedules_medications_medication_id",
                        column: x => x.medication_id,
                        principalSchema: "public",
                        principalTable: "medications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_preferences",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    subject_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    medication_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    lead_minutes = table.Column<short>(type: "smallint", nullable: false),
                    repeat_interval_minutes = table.Column<short>(type: "smallint", nullable: true),
                    max_repeats = table.Column<short>(type: "smallint", nullable: true),
                    quiet_hours_start = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    quiet_hours_end = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    channel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_preferences", x => x.id);
                    table.CheckConstraint("ck_notification_preferences_lead_minutes", "lead_minutes >= 0");
                    table.CheckConstraint("ck_notification_preferences_quiet_hours", "(quiet_hours_start IS NULL) = (quiet_hours_end IS NULL)");
                    table.CheckConstraint("ck_notification_preferences_repeat", "repeat_interval_minutes IS NULL OR repeat_interval_minutes > 0");
                    table.ForeignKey(
                        name: "fk_notification_preferences_app_users_recipient_user_id",
                        column: x => x.recipient_user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notification_preferences_app_users_subject_user_id",
                        column: x => x.subject_user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notification_preferences_medications_medication_id",
                        column: x => x.medication_id,
                        principalSchema: "public",
                        principalTable: "medications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dose_occurrences",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    medication_id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_local_date = table.Column<DateOnly>(type: "date", nullable: false),
                    scheduled_local_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    scheduled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    original_scheduled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    taken_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delay_minutes = table.Column<int>(type: "integer", nullable: true),
                    is_retroactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    record_source = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    occurrence_index = table.Column<int>(type: "integer", nullable: false),
                    generation_version = table.Column<int>(type: "integer", nullable: false),
                    superseded_by_occurrence_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dose_occurrences", x => x.id);
                    table.CheckConstraint("ck_dose_occurrences_generation_version", "generation_version >= 1");
                    table.CheckConstraint("ck_dose_occurrences_occurrence_index", "occurrence_index >= 0");
                    table.CheckConstraint("ck_dose_occurrences_superseded_requires_target", "status <> 'Superseded' OR superseded_by_occurrence_id IS NOT NULL");
                    table.CheckConstraint("ck_dose_occurrences_taken_requires_instant", "status <> 'Taken' OR taken_at_utc IS NOT NULL");
                    table.ForeignKey(
                        name: "fk_dose_occurrences_app_users_recorded_by_user_id",
                        column: x => x.recorded_by_user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_dose_occurrences_app_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_dose_occurrences_dose_occurrences_superseded_by_occurrence_",
                        column: x => x.superseded_by_occurrence_id,
                        principalSchema: "public",
                        principalTable: "dose_occurrences",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_dose_occurrences_medication_schedules_schedule_id",
                        column: x => x.schedule_id,
                        principalSchema: "public",
                        principalTable: "medication_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_dose_occurrences_medications_medication_id",
                        column: x => x.medication_id,
                        principalSchema: "public",
                        principalTable: "medications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "schedule_time_slots",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    time_of_day = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    slot_order = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedule_time_slots", x => x.id);
                    table.CheckConstraint("ck_schedule_time_slots_slot_order", "slot_order >= 0");
                    table.ForeignKey(
                        name: "fk_schedule_time_slots_medication_schedules_schedule_id",
                        column: x => x.schedule_id,
                        principalSchema: "public",
                        principalTable: "medication_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_deliveries",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    recipient_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notification_preference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dose_occurrence_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    scheduled_for_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sent_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    provider_message_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    payload = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_deliveries", x => x.id);
                    table.CheckConstraint("ck_notification_deliveries_attempt_count", "attempt_count >= 0");
                    table.ForeignKey(
                        name: "fk_notification_deliveries_app_users_recipient_user_id",
                        column: x => x.recipient_user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notification_deliveries_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "public",
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_notification_deliveries_dose_occurrences_dose_occurrence_id",
                        column: x => x.dose_occurrence_id,
                        principalSchema: "public",
                        principalTable: "dose_occurrences",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_notification_deliveries_notification_preferences_notificati",
                        column: x => x.notification_preference_id,
                        principalSchema: "public",
                        principalTable: "notification_preferences",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "schedule_adjustments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    triggered_by_occurrence_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    shift_minutes = table.Column<int>(type: "integer", nullable: false),
                    effective_from_date = table.Column<DateOnly>(type: "date", nullable: false),
                    occurrences_affected = table.Column<int>(type: "integer", nullable: false),
                    previous_generation_version = table.Column<int>(type: "integer", nullable: false),
                    new_generation_version = table.Column<int>(type: "integer", nullable: false),
                    applied_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    applied_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedule_adjustments", x => x.id);
                    table.CheckConstraint("ck_schedule_adjustments_occurrences_affected", "occurrences_affected >= 0");
                    table.CheckConstraint("ck_schedule_adjustments_version_progress", "new_generation_version > previous_generation_version");
                    table.ForeignKey(
                        name: "fk_schedule_adjustments_app_users_applied_by_user_id",
                        column: x => x.applied_by_user_id,
                        principalSchema: "public",
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_schedule_adjustments_dose_occurrences_triggered_by_occurren",
                        column: x => x.triggered_by_occurrence_id,
                        principalSchema: "public",
                        principalTable: "dose_occurrences",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_schedule_adjustments_medication_schedules_schedule_id",
                        column: x => x.schedule_id,
                        principalSchema: "public",
                        principalTable: "medication_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_app_users_parent_id",
                schema: "public",
                table: "app_users",
                column: "parent_id",
                filter: "parent_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_app_users_role",
                schema: "public",
                table: "app_users",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "ux_app_users_email",
                schema: "public",
                table: "app_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_app_users_username",
                schema: "public",
                table: "app_users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_child_invites_child_user_id",
                schema: "public",
                table: "child_invites",
                column: "child_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_child_invites_status_expires_at_utc",
                schema: "public",
                table: "child_invites",
                columns: new[] { "status", "expires_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_child_invites_pending_email",
                schema: "public",
                table: "child_invites",
                columns: new[] { "parent_user_id", "invited_email" },
                unique: true,
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "ix_devices_user_id",
                schema: "public",
                table: "devices",
                column: "user_id",
                filter: "is_active");

            migrationBuilder.CreateIndex(
                name: "ux_devices_push_token",
                schema: "public",
                table: "devices",
                column: "push_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dose_occurrences_medication_id_scheduled_at_utc",
                schema: "public",
                table: "dose_occurrences",
                columns: new[] { "medication_id", "scheduled_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_dose_occurrences_pending_scheduled_at_utc",
                schema: "public",
                table: "dose_occurrences",
                columns: new[] { "status", "scheduled_at_utc" },
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "ix_dose_occurrences_recorded_by_user_id",
                schema: "public",
                table: "dose_occurrences",
                column: "recorded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_dose_occurrences_superseded_by_occurrence_id",
                schema: "public",
                table: "dose_occurrences",
                column: "superseded_by_occurrence_id");

            migrationBuilder.CreateIndex(
                name: "ix_dose_occurrences_user_id_scheduled_local_date",
                schema: "public",
                table: "dose_occurrences",
                columns: new[] { "user_id", "scheduled_local_date" });

            migrationBuilder.CreateIndex(
                name: "ux_dose_occurrences_schedule_generation_index",
                schema: "public",
                table: "dose_occurrences",
                columns: new[] { "schedule_id", "generation_version", "occurrence_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_health_conditions_user_id",
                schema: "public",
                table: "health_conditions",
                column: "user_id",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_medication_schedules_generated_through_date",
                schema: "public",
                table: "medication_schedules",
                column: "generated_through_date",
                filter: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_medication_schedules_medication_id",
                schema: "public",
                table: "medication_schedules",
                column: "medication_id");

            migrationBuilder.CreateIndex(
                name: "ix_medication_schedules_user_id",
                schema: "public",
                table: "medication_schedules",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_medications_user_id_is_active",
                schema: "public",
                table: "medications",
                columns: new[] { "user_id", "is_active" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_notification_deliveries_device_id",
                schema: "public",
                table: "notification_deliveries",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_deliveries_dose_occurrence_id",
                schema: "public",
                table: "notification_deliveries",
                column: "dose_occurrence_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_deliveries_notification_preference_id",
                schema: "public",
                table: "notification_deliveries",
                column: "notification_preference_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_deliveries_recipient_user_id_created_at",
                schema: "public",
                table: "notification_deliveries",
                columns: new[] { "recipient_user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_notification_deliveries_scheduled",
                schema: "public",
                table: "notification_deliveries",
                columns: new[] { "status", "scheduled_for_utc" },
                filter: "status = 'Scheduled'");

            migrationBuilder.CreateIndex(
                name: "ix_notification_preferences_medication_id",
                schema: "public",
                table: "notification_preferences",
                column: "medication_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_preferences_recipient_user_id",
                schema: "public",
                table: "notification_preferences",
                column: "recipient_user_id");

            migrationBuilder.CreateIndex(
                name: "ux_notification_preferences_default",
                schema: "public",
                table: "notification_preferences",
                columns: new[] { "subject_user_id", "recipient_user_id", "kind" },
                unique: true,
                filter: "medication_id IS NULL AND deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_notification_preferences_medication_override",
                schema: "public",
                table: "notification_preferences",
                columns: new[] { "subject_user_id", "recipient_user_id", "medication_id", "kind" },
                unique: true,
                filter: "medication_id IS NOT NULL AND deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_report_exports_requested_by_user_id",
                schema: "public",
                table: "report_exports",
                column: "requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_report_exports_subject_user_id_generated_at_utc",
                schema: "public",
                table: "report_exports",
                columns: new[] { "subject_user_id", "generated_at_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_schedule_adjustments_applied_by_user_id",
                schema: "public",
                table: "schedule_adjustments",
                column: "applied_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_schedule_adjustments_schedule_id_applied_at_utc",
                schema: "public",
                table: "schedule_adjustments",
                columns: new[] { "schedule_id", "applied_at_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_schedule_adjustments_triggered_by_occurrence_id",
                schema: "public",
                table: "schedule_adjustments",
                column: "triggered_by_occurrence_id");

            migrationBuilder.CreateIndex(
                name: "ux_schedule_time_slots_schedule_id_slot_order",
                schema: "public",
                table: "schedule_time_slots",
                columns: new[] { "schedule_id", "slot_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_symptom_records_user_id_recorded_at_utc",
                schema: "public",
                table: "symptom_records",
                columns: new[] { "user_id", "recorded_at_utc" },
                descending: new[] { false, true },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_vital_readings_user_id_type_measured_at_utc",
                schema: "public",
                table: "vital_readings",
                columns: new[] { "user_id", "type", "measured_at_utc" },
                descending: new[] { false, false, true },
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "child_invites",
                schema: "public");

            migrationBuilder.DropTable(
                name: "health_conditions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "notification_deliveries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_exports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "schedule_adjustments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "schedule_time_slots",
                schema: "public");

            migrationBuilder.DropTable(
                name: "symptom_records",
                schema: "public");

            migrationBuilder.DropTable(
                name: "vital_readings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "devices",
                schema: "public");

            migrationBuilder.DropTable(
                name: "notification_preferences",
                schema: "public");

            migrationBuilder.DropTable(
                name: "dose_occurrences",
                schema: "public");

            migrationBuilder.DropTable(
                name: "medication_schedules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "medications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "app_users",
                schema: "public");
        }
    }
}
