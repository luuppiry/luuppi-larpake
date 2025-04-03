import {
    ATTENDANCE_KEY_HEADER,
    ATTENDANCE_KEY_LENGTH,
    INVITE_KEY_LENGTH,
    Q_ATTENDANCE_KEY,
    Q_GROUP_ID,
    Q_INVITE_KEY,
} from "./constants.js";
import { LANG_EN, LANG_FI } from "./helpers.js";

export function parseInviteLink(inviteCode: string | null, groupId: number | null = null) {
    if (!inviteCode || inviteCode.length !== INVITE_KEY_LENGTH) {
        return null;
    }
    return `${getUrlHostPart()}/join_invite.html?${Q_INVITE_KEY}=${inviteCode}&${Q_GROUP_ID}=${
        groupId ? groupId : "unknown"
    }`;
}

export function parseAttendanceLink(attendanceKey: string | null) {
    const length = ATTENDANCE_KEY_HEADER.length + ATTENDANCE_KEY_LENGTH;
    if (!attendanceKey || attendanceKey.length !== length) {
        return null;
    }
    return `${getUrlHostPart()}/complete.html?${Q_ATTENDANCE_KEY}=${attendanceKey}`;
}

export function parseValidInviteCodeToKey(inviteCode: string): string | null {
    const isKeyMalformed = inviteCode.length !== INVITE_KEY_LENGTH;
    return isKeyMalformed ? null : inviteCode;
}

function getUrlHostPart() {
    const url = window.location.href;
    const host = window.location.origin;
    const hostIndex = url.indexOf(host) + host.length + 1;
    const isFi = url.slice(hostIndex, hostIndex + "en".length) != "en";

    return `${host}/${isFi ? LANG_FI : LANG_EN}`;
}
