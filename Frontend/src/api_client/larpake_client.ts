import HttpClient from "./http_client.js";
import { Larpake, LarpakeTask, Section } from "../models/larpake.js";
import { Container, IdObject } from "../models/common.js";
import { ThrowIfNull } from "../helpers.js";
import RequestEngine from "./request_engine.js";

type Ids = {
    ids: number[];
};

export default class LarpakeClient extends RequestEngine {
    constructor(client: HttpClient | null = null) {
        super(client);
    }

    async getById(id: number, minimize: boolean = false): Promise<Larpake | null> {
        const query = new URLSearchParams();
        query.append("DoMinimize", minimize ? "true" : "false");
        query.append("LarpakeIds", id.toString());

        const response = await this.client.get(`api/larpakkeet`, query);
        const records: Container<Larpake[]> | null = await response.json();
        return records?.data[0] ?? null;
    }

    async getAll(minimize: boolean): Promise<Larpake[] | null> {
        const query = new URLSearchParams();
        query.append("DoMinimize", minimize ? "true" : "false");

        const response = await this.client.get("api/larpakkeet", query);
        if (!response.ok) {
            console.error("Failed to fetch Lärpäkkeet");
            return null;
        }
        const container: Container<Larpake[]> = await response.json();
        return container.data;
    }

    async getOwn(minimize: boolean = false): Promise<Larpake[] | null> {
        const query = new URLSearchParams();
        query.append("minimize", minimize ? "true" : "false");

        const response = await this.client.get("api/larpakkeet/own", query);
        if (!response.ok) {
            console.warn(response);
            return null;
        }
        const container: Container<Larpake[]> = await response.json();
        return container.data;
    }

    async getTasksByLarpakeId(larpakeId: number) {
        /* Requires Admin */
        ThrowIfNull(larpakeId);

        const params = new URLSearchParams();
        params.append("LarpakeId", larpakeId.toString());
        params.append("pageSize", "100");

        const response = await this.client.get("api/larpake-tasks", params);

        if (!response.ok) {
            console.warn(response);
            return null;
        }
        const tasks: Container<LarpakeTask[]> = await response.json();
        return tasks.data;
    }

    async getTasks(
        larpakeId: number | null = null,
        taskIds: number[] | null = null
    ): Promise<LarpakeTask[] | null> {
        const query = new URLSearchParams();
        // Different search params
        // query.append("userId", "<guid>");
        // query.append("groupId", "<num>");
        // query.append("sectionId", "<num>");
        // query.append("isCancelled", "<num>");
        // query.append("pageSize", "<num>");
        // query.append("pageOffset", "<num>");

        if (larpakeId) {
            query.append("LarpakeId", larpakeId.toString());
        }
        if (taskIds) {
            taskIds.forEach((x) => query.append("LarpakeTaskIds", x.toString()));
        }

        const response = await this.client.get("api/larpake-tasks", query);
        if (!response.ok) {
            console.warn(response);
            return null;
        }

        const tasks: Container<LarpakeTask[]> = await response.json();
        return tasks.data;
    }

    async getEvents(taskId: number): Promise<number[] | null> {
        if (taskId == undefined) {
            throw new Error("Task id must be defined.");
        }

        const response = await this.client.get(
            `/api/larpake-events/${taskId}/attendance-opportunities`
        );

        if (!response.ok) {
            console.warn(response);
            return null;
        }

        const events: Ids = await response.json();
        return events.ids;
    }

    async getTaskById(taskId: number): Promise<LarpakeTask | null> {
        if (!taskId) {
            console.warn("Cannot fetch provided null task id");
            return null;
        }

        return await this.get<LarpakeTask>({
            url: `api/larpake-tasks/${taskId}`,
            params: null,
            failMessage: `Failed to fetch task with id ${taskId}`,
            isContainerType: false,
        });
    }

    async getSectionById(sectionId: number): Promise<Section | null> {
        return await this.get({
            url: `api/larpakkeet/section/${sectionId}`,
            params: null,
            failMessage: `Failed to fetch section with id ${sectionId}`,
            isContainerType: false,
        });
    }

    async uploadLarpakeCommonDataOnly(larpake: Larpake): Promise<number> {
        ThrowIfNull(larpake);

        if (larpake.id > 0) {
            // UPDATE Existing
            const response = await this.client.put(`api/larpakkeet/${larpake.id}`, larpake);
            if (response.ok) {
                return larpake.id;
            }
            console.warn(await response.json());
            return -1;
        }

        // Insert new
        const response = await this.client.post("api/larpakkeet", larpake);
        if (response.ok) {
            const id: IdObject = await response.json();
            return id.id;
        }
        console.warn(await response.json());
        return -1;
    }

    async uploadLarpakeSectionsOnly(larpake: Larpake): Promise<number> {
        ThrowIfNull(larpake);

        const existing = await this.getById(larpake.id);
        if (!existing) {
            throw new Error(
                "Save common data first, to create Larpake." +
                    " If you have already sent common data first or larpake should exists, there might be a bug."
            );
        }

        const existingIds = new Set(existing.sections?.map((x) => x.id));
        const updateables = larpake.sections?.filter((x) => existingIds.has(x.id)) ?? [];

        const countUpdated = await this.#updateSections(larpake.id, updateables);
        console.log(`Updated ${countUpdated} existing sections`);

        const newOnes = larpake.sections?.filter((x) => !existingIds.has(x.id)) ?? [];
        const countCreated = await this.#createSections(larpake.id, newOnes);

        console.log(`Created ${countCreated} new sections`);
        return countUpdated + countCreated;
    }

    async #createSections(larpakeId: number, sections: Section[]): Promise<number> {
        for (const section of sections) {
            const response = await this.client.post(
                `api/larpakkeet/${larpakeId}/sections`,
                section
            );
            if (!response.ok) {
                throw new Error(await response.json());
            }

            const sectionId: IdObject = await response.json();
            for (const task of section.tasks) {
                task.larpakeSectionId = sectionId.id;
                this.#createTask(task);
            }
        }

        return sections.length;
    }

    async #updateSections(larpakeId: number, sections: Section[]): Promise<number> {
        // Calculate which should be deleted, load before adding new ones
        const existingTasks: LarpakeTask[] | null = await this.getTasks(larpakeId);
        if (existingTasks == null) {
            return sections.length;
        }

        for (const section of sections) {
            const response = await this.client.put(`api/larpakkeet/${larpakeId}/sections`, section);
            if (!response.ok) {
                throw new Error(await response.json());
            }

            for (const task of section.tasks) {
                await this.#uploadTask(section.id, task);
                console.log(`Uploaded task ${task.textData[0]?.title}`);
            }
        }

        // Cancel deleted tasks
        const validIds = new Set(
            sections
                .flatMap((x) => x.tasks)
                .filter((x) => x != undefined)
                .filter((x) => x.id > 0)
                .map((x) => x.id)
        );

        const deletedIds = existingTasks.map((x) => x.id).filter((x) => !validIds.has(x));
        for (const taskId of deletedIds) {
            const deleted = await this.client.post(`api/larpake-tasks/${taskId}/cancel`);
            if (!deleted.ok) {
                console.warn(`Failed to delete task ${taskId}.`);
                console.log(await deleted.json());
            }
        }

        return sections.length;
    }

    async #uploadTask(sectionId: number, task: LarpakeTask) {
        task.larpakeSectionId = sectionId;
        if (task.id < 0) {
            // Create
            await this.#createTask(task);
            return;
        }
        // Update
        await this.#updateTask(task);
    }

    async #createTask(task: LarpakeTask) {
        const response = await this.client.post(`api/larpake-tasks`, task);
        if (!response.ok) {
            const error = await response.json();

            throw new Error(error);
        }
    }

    async #updateTask(task: LarpakeTask) {
        const response = await this.client.put(`api/larpake-tasks/${task.id}`, task);
        if (!response.ok) {
            throw new Error(await response.json());
        }
    }
}
