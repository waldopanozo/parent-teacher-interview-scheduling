import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const root = path.join(__dirname, '..');
const target = path.join(root, 'src', 'environments', 'environment.ts');

const id = process.env.GOOGLE_WEB_CLIENT_ID ?? '';
const content = `export const environment = {
  production: true,
  googleClientId: ${JSON.stringify(id)},
  apiBaseUrl: '/api'
};
`;

fs.writeFileSync(target, content, 'utf8');
