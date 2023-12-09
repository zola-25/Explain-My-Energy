export class StorageHelper {

    private static buff_to_base64(buff: ArrayBuffer): string {
        return btoa(new Uint8Array(buff).reduce((data, byte) => data + String.fromCharCode(byte), ''));
    }

    private static base64_to_buff(base64: string): ArrayBuffer {
        return Uint8Array.from(atob(base64), c => c.charCodeAt(0)).buffer;
    }

    private static getEncoder(): TextEncoder {
        return new TextEncoder();
    }

    private static getDecoder(): TextDecoder {
        return new TextDecoder();
    }

    public static async encrypt(data: string, password: string): Promise<string> {
        try {
            const salt = window.crypto.getRandomValues(new Uint8Array(16));
            const iv = window.crypto.getRandomValues(new Uint8Array(12));
            const passwordKey = await this.getPasswordKey(password);
            const aesKey = await this.deriveKey(passwordKey, salt, "encrypt");
            const encrypted = await window.crypto.subtle.encrypt(
                {
                    name: "AES-GCM",
                    iv
                },
                aesKey,
                this.getEncoder().encode(data)
            );
            const encryptedData = new Uint8Array(encrypted);
            const buff = new Uint8Array(salt.byteLength + iv.byteLength + encryptedData.byteLength);
            buff.set(salt, 0);
            buff.set(iv, salt.byteLength);
            buff.set(encryptedData, salt.byteLength + iv.byteLength);
            const base64buff = this.buff_to_base64(buff.buffer);
            return base64buff;
        }
        catch (e: any) {
            throw Error("Error encountered encrypting data");
        }
    }

    public static async decrypt(data: string, password: string): Promise<string> {
        try {
            const encryptDataBuff = this.base64_to_buff(data);
            const salt = encryptDataBuff.slice(0, 16);
            const iv = encryptDataBuff.slice(16, 28);
            const encryptedData = encryptDataBuff.slice(28);
            const passwordKey = await this.getPasswordKey(password);
            const aesKey = await this.deriveKey(passwordKey, salt, "decrypt");

            const decrypted = await window.crypto.subtle.decrypt(
                {
                    name: "AES-GCM",
                    iv: iv
                },
                aesKey,
                encryptedData
            );
            return this.getDecoder().decode(decrypted);
        }
        catch (e: any) {
            throw Error("Error encountered when decrypting");
        }
    }


    private static getPasswordKey(password: string): Promise<CryptoKey> {
        return window.crypto.subtle.importKey("raw", this.getEncoder().encode(password), "PBKDF2", false, ["deriveKey"]);
    }

    private static deriveKey(passwordKey: CryptoKey, salt: ArrayBuffer, keyUsage: KeyUsage): Promise<CryptoKey> {
        return window.crypto.subtle.deriveKey(
            {
                name: "PBKDF2",
                salt,
                iterations: 250000,
                hash: "SHA-256"
            },
            passwordKey,
            { name: "AES-GCM", length: 256 },
            false,
            [keyUsage]
        );
    }

    public static Load(): void {
        window["StorageHelper"] = StorageHelper;
    }
}