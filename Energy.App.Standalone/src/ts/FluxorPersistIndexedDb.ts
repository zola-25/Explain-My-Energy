export interface IDbState {
    stateName: string;
    state: any;
}


export default class FluxorPersistIndexedDb {

    private DATABASE_NAME: string = "FluxorPersistIndexDB";


    private open(stateName: string): IDBDatabase {
        const dbName = this.DATABASE_NAME;

        const request = window.indexedDB.open(dbName);
        let db: IDBDatabase;
        request.onsuccess = function () {
            if (request.result.objectStoreNames.contains(stateName)) {
                db = request.result;
            } else {
                const version = request.result.version
                request.result.close();

                request.result.onclose = function () {

                    const upgradeRequest = window.indexedDB.open(dbName, version + 1);
                    upgradeRequest.onupgradeneeded = function (event) {
                        db = upgradeRequest.result;
                        db.createObjectStore(stateName, { keyPath: "stateName" });
                    };
                }
            }
        }
        return db;

    }

    public saveState(state: IDbState): void {
        let db: IDBDatabase;
        try {
            db = this.open(state.stateName);

            const tx = db.transaction(state.stateName, "readwrite");
            const store = tx.objectStore(state.stateName);
            store.put(state);
            db.close();
        }
        finally {
            if (db) {
                db.close();
            }
        }
    };

    public async getState(stateName: string): Promise<IDbState> {
        let db: IDBDatabase;
        const request = new Promise<IDbState>((resolve) => {
            db = this.open(stateName);

            const tx = db.transaction(stateName, "readonly");
            const store = tx.objectStore(stateName);
            const request = store.get(stateName);
            request.onsuccess = function () {
                resolve(request.result);
            };

        }).finally(() => {
            if (db) {
                db.close()
            }
        });

        const result = await request;
        return result;
    }

    public static Load(): void {
        window["FluxorPersistIndexedDb"] = new FluxorPersistIndexedDb();
    }
}
