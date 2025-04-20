import {
    ATTENDANCE_CODE_HEADER,
    ATTENDANCE_KEY_LENGTH,
    INVITE_KEY_LENGTH,
    Q_ATTENDANCE_KEY,
    Q_GROUP_ID,
    Q_INVITE_KEY,
} from "./constants.js";
import { LANG_EN, LANG_FI } from "./helpers.js";

/* Key refers to 6-8 character span that is the identifier.
 * Code referes to Header+Key combination inside link */
const ATTENDANCE_CODE_LENGTH = ATTENDANCE_CODE_HEADER.length + ATTENDANCE_KEY_LENGTH;

export function parseInviteLink(inviteCode: string | null, groupId: number | null = null) {
    if (!inviteCode || inviteCode.length !== INVITE_KEY_LENGTH) {
        return null;
    }
    const formattedCode = inviteCode.toUpperCase();

    const params = new URLSearchParams();
    params.append(Q_INVITE_KEY, formattedCode);
    if (groupId){
        params.append(Q_GROUP_ID, groupId.toString());
    }

    return `${getUrlHostPart()}/join_invite.html?${params.toString()}`
}

export function parseAttendanceLink(attendanceKey: string | null) {
    const length = ATTENDANCE_CODE_LENGTH;
    if (!attendanceKey || attendanceKey.length !== length) {
        return null;
    }
    return `${getUrlHostPart()}/complete.html?${Q_ATTENDANCE_KEY}=${attendanceKey}`;
}

export function parseValidInviteCodeToKey(inviteCode: string): string | null {
    const isKeyMalformed = inviteCode.length !== INVITE_KEY_LENGTH;
    return isKeyMalformed ? null : inviteCode;
}

export function removeAttendanceCodeHeader(attendanceCode: string): string | null {
    const isValid =
        attendanceCode &&
        attendanceCode.startsWith(ATTENDANCE_CODE_HEADER) &&
        attendanceCode.length === ATTENDANCE_CODE_LENGTH;
    if (!isValid) {
        return null;
    }
    return attendanceCode.slice(ATTENDANCE_CODE_HEADER.length);
}

function getUrlHostPart() {
    const url = window.location.href;
    const host = window.location.origin;
    const hostIndex = url.indexOf(host) + host.length + 1;
    const isFi = url.slice(hostIndex, hostIndex + "en".length) != "en";

    return `${host}/${isFi ? LANG_FI : LANG_EN}`;
}
