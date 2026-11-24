CREATE TABLE semesters
(
    id              BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    current_day     INTEGER NOT NULL DEFAULT 0 CHECK (current_day >= 0),
    pleasure        DOUBLE PRECISION NOT NULL DEFAULT 0,
    missed_lessons  INTEGER NOT NULL DEFAULT 0 CHECK (missed_lessons >= 0),
    days_completed  INTEGER NOT NULL DEFAULT 0 CHECK (days_completed >= 0),
    is_expelled     BOOLEAN NOT NULL DEFAULT FALSE,
    status          VARCHAR(20) NOT NULL DEFAULT 'in_progress'
                    CHECK (status IN ('in_progress', 'completed', 'expelled')),
    random_state    BIGINT NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT ck_semesters_days
        CHECK (days_completed <= current_day)
);

CREATE TABLE teachers
(
    id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    semester_id BIGINT NOT NULL
                REFERENCES semesters(id) ON DELETE CASCADE,
    subject     INTEGER NOT NULL CHECK (subject BETWEEN 0 AND 5),
    rule        INTEGER NOT NULL CHECK (rule BETWEEN 0 AND 2),
    subject_a   INTEGER NULL CHECK (subject_a BETWEEN 0 AND 5),
    subject_b   INTEGER NULL CHECK (subject_b BETWEEN 0 AND 5),

    CONSTRAINT uq_teachers_semester_subject
        UNIQUE (semester_id, subject),

    CONSTRAINT ck_teachers_rule_subjects
        CHECK (
            (rule = 0 AND subject_a IS NULL AND subject_b IS NULL)
            OR
            (rule = 1 AND subject_a IS NOT NULL AND subject_b IS NULL)
            OR
            (rule = 2 AND subject_a IS NOT NULL AND subject_b IS NOT NULL
                      AND subject_a <> subject_b)
        )
);

CREATE TABLE day_results
(
    id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    semester_id BIGINT NOT NULL
                REFERENCES semesters(id) ON DELETE CASCADE,
    day_number  INTEGER NOT NULL CHECK (day_number > 0),
    subject     INTEGER NOT NULL CHECK (subject BETWEEN 0 AND 5),
    was_asked   BOOLEAN NOT NULL,

    CONSTRAINT uq_day_results_semester_day_subject
        UNIQUE (semester_id, day_number, subject)
);

CREATE INDEX ix_day_results_semester_day
    ON day_results (semester_id, day_number);

CREATE INDEX ix_teachers_semester
    ON teachers (semester_id);