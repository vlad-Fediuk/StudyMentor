import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '10s', target: 10 },
    { duration: '15s', target: 50 },
    { duration: '15s', target: 100 },
    { duration: '10s', target: 0 },
  ],
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<1000'],
  },
};

export default function () {
  const response = http.get('http://host.docker.internal:5132/groups');
  check(response, {
    'status is 200': (r) => r.status === 200,
  });
  sleep(0.1);
}
