// Query parameters
export const Q_LARPAKE_ID = "LarpakeId";
export const Q_TASK_ID = "TaskId";
export const Q_SECTION_ID = "SectionId";
export const Q_PAGE = "Page";

export const Q_LAST_PAGE = "LastPage";
export const Q_OF_PAGES = "OfPages";

export const Q_GROUP_ID = "GroupId";

export const Q_NEW = "New";

export const Q_PAGE_SIZE = "PageSize";
export const Q_PAGE_OFFSET = "PageOffset";
export const Q_SEARCH = "Search";

export const Q_INVITE_KEY = "InviteKey";
export const Q_ATTENDANCE_KEY = "AttendanceKey";

export const ATTENDANCE_KEY_LENGTH = 6;
export const ATTENDANCE_KEY_HEADER = "lak-v1_";

export const INVITE_KEY_LENGTH = 8;
export const Q_KEY_MALFORMED = "KeyMalformed";
export const Q_KEY_EXPIRED = "KeyExpired";

export const SIGNATURE_INFO = {
    WIDTH: 450,
    HEIGTH: 100,
    LINE_WIDTH: 2,
    STROKE_STYLE: "black",
    LINE_CAP: "round"
}

export enum SERVER_STATUSES {
    UNDEFINED = 0,
    // Id errors
    ID_ERROR = 1100,
    INVALID_ID = 1101,
    ID_NOT_FOUND = 1102,

    // Integration
    EXTERNAL_SERVER_ERROR = 1500,
    INTEGRATION_DB_WRITE_FAILED = 1501,
    // Auth
    AUTH_ERROR = 1600,
    INVALID_JWT = 1601,

    // Internal server error
    UNKNOWN_SERVER_ERROR = 1700,

    // User action forbidden for user
    USER_STATUS_TUTOR = 1801,
    USER_NOT_ATTENDING = 1802,
}

export const MAX_SIGNATURES = 10;