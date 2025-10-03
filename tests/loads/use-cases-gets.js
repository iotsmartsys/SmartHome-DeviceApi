import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '1m30s', target: 1000 },
        { duration: '30s', target: 2000 },
    ],
};

export default function () {
    const res = http.get('http://localhost:8081/api/v1/capabilities');
    check(res, { 'status was 200': (r) => r.status == 200 });
    sleep(1);
}