type Creadentials = {
    accessToken: string;
};

let creadentials: Creadentials | null = null;
const userId = "0194ad9a-c487-705e-90c0-2a046022a0c0";
const api = "https://localhost:7267/api";

async function makeRequest() {
    const query = new URLSearchParams();
    query.append("minimize", "false");

    let url = `${api}/larpakkeet/own?${query.toString()}`;
    let request = {
        method: "GET",
        headers: new Headers(),
    };
    request.headers.append("Content-Type", "application/json");

    const data = await runWithMiddleware(url, request);
}

async function runWithMiddleware(url: string, request: any) {
    if (creadentials !== null) {
        request.headers.append(
            "Authorization",
            `Bearer ${creadentials.accessToken}`
        );
    }

    const first = await fetch(url, request);
    if (!first.ok) {
        if (first.status === 401) {
            // invalid token
            creadentials = await fetchLogin(userId);
        } else {
            // Failed
            console.log(first);
            throw new Error("Request failed");
        }
    } else {
        return await first.json();
    }

    // Add new auth token
    request.headers.append(
        "Authorization",
        `Bearer ${creadentials?.accessToken}`
    );
    const second = await fetch(url, request);
    if (!second.ok) {
        throw new Error("Invalid credentials");
    }
    return await second.json();
}

async function fetchLogin(userId: string) {
    const response = await fetch(`${api}/authentication/login/dummy`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
            userId: userId,
        }),
    });

    if (!response.ok) {
        throw new Error("Network error");
    }
    return await response.json();
}
