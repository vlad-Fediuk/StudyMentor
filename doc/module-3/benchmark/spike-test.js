import http from 'k6/http';
import { check, sleep } from 'k6';

const baseUrl = __ENV.BASE_URL || 'http://localhost:5132';
const endpoint = __ENV.ENDPOINT || '/lectures';

export const options = {
  stages: [
    { duration: '10s', target: 10 },
    { duration: '5s', target: 300 },
    { duration: '20s', target: 300 },
    { duration: '5s', target: 10 },
    { duration: '5s', target: 0 },
  ],
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<1000'],
  },
};

export default function () {
  const response = http.get(`${baseUrl}${endpoint}`);
  check(response, {
    'status is 200': (r) => r.status === 200,
  });
  sleep(1);
}
