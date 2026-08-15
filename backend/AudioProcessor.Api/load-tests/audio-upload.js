import http from 'k6/http';
import { check } from 'k6';

const API_URL = __ENV.API_URL || 'http://localhost:5262';

// open() only works in the init stage (global scope), so the file is read
// once here and reused as bytes on every iteration.
const audioFile = open('../sample-files/sample3.mp3', 'b');

export const options = {
  vus: 5,
  duration: '30s',
};

export default function () {
  const payload = {
    file: http.file(audioFile, 'sample3.mp3', 'audio/mpeg'),
  };

  const res = http.post(`${API_URL}/api/audio`, payload);

  check(res, {
    'status is 200 or 202': (r) => r.status === 200 || r.status === 202,
  });
}
