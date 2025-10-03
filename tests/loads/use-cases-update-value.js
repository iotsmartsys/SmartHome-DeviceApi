import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '1m30s', target: 1000 },
        { duration: '30s', target: 2000 },
    ],
};

export default function () {
    const url = 'http://localhost:8081/api/v1/capabilities/value';

    const headers = { 'Content-Type': 'application/json' };
    const data = {
        capability_name: "lampadaCorredor",
        value: "on"
    };

    const res = http.patch(url, JSON.stringify(data), { headers: headers });
    check(res, { 'status was 204': (r) => r.status == 204 });
    sleep(1);
}