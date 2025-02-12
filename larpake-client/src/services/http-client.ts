

type Creadentials = {
    accessToken: string;
};

let creadentials: Creadentials | null = null;

const userId = process.env.REACT_APP_DEBUG_USER!;

export default async function makeRequest() {
    const query = new URLSearchParams();
    query.append("minimize", "false");

    let url = `${
        process.env.REACT_APP_API_HOST
    }/larpakkeet/own?${query.toString()}`;
    let request = {
        method: "GET",
        headers: new Headers(),
    };
    request.headers.append("Content-Type", "application/json");

    return await runWithMiddleware(url, request);
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
            creadentials = await fetchDummyLogin(userId);
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





async function fetchDummyLogin(userId: string) {
    const response = await fetch(
        `${process.env.REACT_APP_API_HOST}/authentication/login/dummy`,
        {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                userId: userId,
            }),
        }
    );

    if (!response.ok) {
        throw new Error("Network error");
    }
    return await response.json();
}
