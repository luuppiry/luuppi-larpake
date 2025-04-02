import { INVITE_KEY_LENGTH, Q_GROUP_ID, Q_INVITE_KEY } from "./constants.js";

export function parseInviteLink(inviteCode: string | null, groupId: number | null = null) {
    if (!inviteCode && inviteCode?.length == INVITE_KEY_LENGTH) {
        return null;
    }
    const url = window.location.href;
    const host = window.location.origin;

    const hostIndex = url.indexOf(host) + host.length + 1;
    const isFi = url.slice(hostIndex, hostIndex + "en".length) != "en";

    return `${host}/${
        isFi ? "fi" : "en"
    }/join_invite.html?${Q_INVITE_KEY}=${inviteCode}&${Q_GROUP_ID}=${
        groupId ? groupId : "unknown"
    }`;
}
