// Query parameters
export const Q_LARPAKE_ID = "larpakeid";
export const Q_TASK_ID = "taskid";
export const Q_SECTION_ID = "sectionid";
export const Q_PAGE = "page";

export const Q_LAST_PAGE = "lastpage";
export const Q_OF_PAGES = "ofpages";

export const Q_GROUP_ID = "groupid";

export const Q_NEW = "new";
export const Q_STATUS = "status";

export const Q_PAGE_SIZE = "pagesize";
export const Q_PAGE_OFFSET = "pageoffset";
export const Q_SEARCH = "search";

export const Q_INVITE_KEY = "invitekey";
export const Q_ATTENDANCE_KEY = "attendancekey";

export const ATTENDANCE_KEY_LENGTH = 6;
export const ATTENDANCE_CODE_HEADER = "lak-v1_";

export const INVITE_KEY_LENGTH = 8;
export const Q_KEY_MALFORMED = "keymalformed";
export const Q_KEY_EXPIRED = "keyexpired";

export const Q_PARAM = "param";

export const UI_HEADER_ID = "ui-header-component"

export const SIGNATURE_INFO = {
    WIDTH: 450,
    HEIGTH: 100,
    LINE_WIDTH: 2,
    STROKE_STYLE: "black",
    LINE_CAP: "round",
};

export enum SERVER_STATUS {
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

/* Do not full trust these permission numbers, 
 * Always check correct numbers from API
 * side if needed for things like auth */
export enum Permissions {
    Freshman = 6,
    Tutor = 766,
    Admin = 4194302,
    Sudo = 2147483647
}