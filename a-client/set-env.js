const fs = require('fs');
const path = require('path');

const targetPath = path.join(__dirname, 'src/environments/environment.prod.ts');
const apiUrl = process.env.API_URL || process.env.NEXT_PUBLIC_API_URL || 'https://studymentor.onrender.com';
const redirectUri = process.env.REDIRECT_URI || 'https://studymentor.pages.dev/auth';

const envConfigFile = `export const environment = {
  production: true,
  apiUrl: '${apiUrl.replace(/\/$/, '')}',
  redirectUri: '${redirectUri}'
};
`;

fs.writeFileSync(targetPath, envConfigFile);
