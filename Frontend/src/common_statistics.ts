import LarpakeClient from "./api_client/larpake_client.js";
import GroupClient from "./api_client/group_client.js";
import { replaceUrlState, getSearchParams } from "./helpers.js";
import { Q_LARPAKE_ID } from "./constants.js";
import StatsClient from "./api_client/stats_client.js";
import { loadConfigFromFile } from "vite";

const client = new LarpakeClient();
const groupClient = new GroupClient();
const statisticsClient = new StatsClient(client.client);

async function main() {
    const params = getSearchParams();
    let larpakeId = params.get(Q_LARPAKE_ID);

    if (!larpakeId) {
        const ids = await client.getOwn(true);
        if (!ids || ids.length < 1) {
            throw new Error("No Larpake id defined and not attending any");
        }
        larpakeId = ids[0].id.toString();
        replaceUrlState((params) => {
            params.set(Q_LARPAKE_ID, larpakeId!);
        });
    }

    // Display points from every group of fuksi
    const groups = await groupClient.getOwnGroups(false);
    const container = document.querySelector('.group-points');
    const lang = document.documentElement.lang || 'en';
    const placeholder = container.querySelector('p');
    if (placeholder) {
      placeholder.remove();
    }
    for (const group of groups) {
      const result = await statisticsClient.getOwnGroupTotalPoints(group.id);
      const points = result?.data ?? 'N/A';

      const p = document.createElement('p');
      if (lang === 'fi') {
        p.innerHTML = `Fuksiryhmäsi <strong>${group.name}</strong> on kerännyt yhteensä <strong>${points}</strong> pistettä.`;
      } else {
        p.innerHTML = `Your group <strong>${group.name}</strong> has gathered in total <strong>${points}</strong> points.`;
      }
      container?.appendChild(p);
    }

    // Total amount of all gathered points in one Larpake
    const larpakeTotalPoints = await statisticsClient.getOwnLarpakeTotalPoints(parseInt(larpakeId ?? ""));
    document.querySelector('strong.total-points-amount.loader')?.classList.remove('loader');
    document.querySelector('strong.total-points-amount').innerText = larpakeTotalPoints?.data;

    // Average fuksi points circle progress
    const larpakePointsAverageStatistics = await statisticsClient.getOwnLarpakePointsAverage(parseInt(larpakeId ?? ""));
    const larpakeTotalPointsStatistics = await statisticsClient.getOwnLarpakeStatistics(parseInt(larpakeId ?? ""));
    const totalPointsInLarpake = larpakeTotalPointsStatistics?.data.reduce((sum, s) => sum + s.totalPoints, 0);
    setProgress(larpakePointsAverageStatistics.data, totalPointsInLarpake);

    // Leading user points
    const leadingUserPoints = await statisticsClient.getLeadingUser(larpakeId ?? "");
    document.querySelector('strong.leading-user-points-amount.loader')?.classList.remove('loader');
    document.querySelector('strong.leading-user-points-amount').innerText = (leadingUserPoints?.data)[0].points.toString();

    // Leading group points
    const leadingGroupPoints = await statisticsClient.getLeadingGroup(larpakeId ?? "");
    document.querySelector('strong.leading-group-points-amount.loader')?.classList.remove('loader');
    document.querySelector('strong.leading-group-points-amount').innerText = (leadingGroupPoints?.data)[0].points.toString();

    // Remove placeholder from count 
    document.querySelector('#days')?.classList.remove('loader');
    document.querySelector('#hours')?.classList.remove('loader');
    document.querySelector('#minutes')?.classList.remove('loader');
    document.querySelector('#seconds')?.classList.remove('loader');
}

function setProgress(current: number, max: number) {
    const circle = document.querySelector('.progress-ring__circle') as SVGCircleElement;
    const percentEl = document.getElementById('progressPercent');
    const valueEl = document.getElementById('progressValue');

    document.querySelector('#progressPercent')?.classList.remove('loader');
    document.querySelector('#progressValue')?.classList.remove('loader');
  
    const radius = circle.r.baseVal.value;
    const circumference = 2 * Math.PI * radius;
  
    const percent = Math.min(current / max, 1);
    const offset = circumference * (1 - percent);
  
    circle.style.strokeDashoffset = offset.toString();
  
    if (percentEl) {
      percentEl.textContent = `${Math.round(percent * 100)}%`;
    }
  
    if (valueEl) {
      valueEl.textContent = `${current} / ${max}`;
    }
  }

main();
