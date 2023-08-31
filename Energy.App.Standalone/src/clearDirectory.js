import fs from 'fs';
import path from 'path';

export function clearDirectory(directoryPath, exclude) {
    const files = fs.readdirSync(directoryPath, { withFileTypes: true });

    files.forEach(file => {
        const filePath = path.join(directoryPath, file.name);

        if (file.isDirectory() && file.name !== exclude) {
            fs.rmSync(filePath, { recursive: true, force: true });
        } else if (file.isFile()) {
            fs.rmSync(filePath);
        }
    });
}
