/**
 * Check if localStorage is supported                       const isSupported: boolean
 * Check if localStorage has an Item                        function hasItem(key: string): boolean
 * Get the amount of space left in localStorage             function getRemainingSpace(): number
 * Get the maximum amount of space in localStorage          function getMaximumSpace(): number
 * Get the used space in localStorage                       function getUsedSpace(): number
 * Get the used space of an item in localStorage            function getItemUsedSpace(): number
 * Backup Assosiative Array                                 interface Backup
 * Get a Backup of localStorage                             function getBackup(): Backup
 * Apply a Backup to localStorage                           function applyBackup(backup: Backup, fClear: boolean = true, fOverwriteExisting: boolean = true)
 * Dump all information of localStorage in the console      function consoleInfo(fShowMaximumSize: boolean = false)
 */

/**
 * Associative-array for localStorage holding key->value
 */
export interface IBackup {
    [index: string]: string;
  }
  
export class LocalStorage {
    /**
     * Flag set true if the Browser supports localStorage, widthout affecting it
     */
    public static isSupported: boolean = (() => {
      try {
        const itemBackup = localStorage.getItem('');
        localStorage.removeItem('');
        localStorage.setItem('', '');
        if (itemBackup === null) {
          localStorage.removeItem('');
        } else {
          localStorage.setItem('', itemBackup);
        }
        return true;
      } catch (e) {
        return false;
      }
    })();
  
    /**
     * Check if localStorage has an Item / exists with the give key
     * @param key the key of the Item
     */
    public static hasItem(key: string): boolean {
      return localStorage.getItem(key) !== null;
    }
  
    /**
     * This will return the left space in localStorage without affecting it's content
     * Might be slow !!!
     */
    public static getRemainingSpace(): number {
      const itemBackup = localStorage.getItem('');
      let increase = true;
      let data = '1';
      let totalData = '';
      let trytotalData = '';
      while (true) {
        try {
          trytotalData = totalData + data;
          localStorage.setItem('', trytotalData);
          totalData = trytotalData;
          if (increase) {
            data += data;
          }
        } catch (e) {
          if (data.length < 2) {
            break;
          }
          increase = false;
          data = data.substr(data.length / 2);
        }
      }
      if (itemBackup === null) {
        localStorage.removeItem('');
      } else {
        localStorage.setItem('', itemBackup);
      }
  
      return totalData.length - (itemBackup !== null ? itemBackup.length : 0);
    }
  
    /**
     * This function returns the maximum size of localStorage without affecting it's content
     * Might be slow !!!
     */
    public static getMaximumSpace(): number {
      const backup = LocalStorage.getBackup();
      localStorage.clear();
      const max = LocalStorage.getRemainingSpace();
      LocalStorage.applyBackup(backup);
      return max;
    }
  
    /**
     * This will return the currently used size of localStorage
     */
    public static getUsedSpace(): number {
      let sum = 0;
  
      for (let i = 0; i < localStorage.length; ++i) {
        const key = localStorage.key(i) as string;
        const value = localStorage.getItem(key) as string;
        sum += key.length + value.length;
      }
  
      return sum;
    }
  
    /**
     * This will return the currently used size of a given Item, returns NaN if key is not found
     * @param key
     */
    public static getItemUsedSpace(key: string): number {
      const value = localStorage.getItem(key);
      if (value === null) {
        return NaN;
      } else {
        return key.length + value.length;
      }
    }
    /**
     * This will return a localStorage-backup (Associative-Array key->value)
     */
    public static getBackup(): IBackup {
      const backup: IBackup = {};
  
      for (let i = 0; i < localStorage.length; ++i) {
        const key = localStorage.key(i) as string;
        const value = localStorage.getItem(key) as string;
        backup[key] = value;
      }
  
      return backup;
    }
  
    /**
     * This will apply a localStorage-Backup (Associative-Array key->value)
     * @param backup            associative-array
     * @param fClear             optional flag to clear all existing storage first. Default: true
     * @param fOverwriteExisting optional flag to replace existing keys. Default: true
     */
    public static applyBackup(backup: IBackup, fClear: boolean = true, fOverwriteExisting: boolean = true) {
      if (fClear === true) {
        localStorage.clear();
      }
  
      for (const key in backup) {
        if (fOverwriteExisting === false && backup[key] !== undefined) {
          continue;
        }
        const value = backup[key];
        localStorage.setItem(key, value);
      }
    }
  
    /**
     * This functions dumps all keys and values of the local Storage to the console,
     * as well as the current size and number of items
     * @param fShowMaximumSize optional, flag show maximum size of localStorage. Default: false
     */
    public static consoleInfo(fShowMaximumSize: boolean = false) {
      let amount = 0;
      let size = 0;
  
      for (let i = 0; i < localStorage.length; ++i) {
        const key = localStorage.key(i) as string;
        const value = localStorage.getItem(key) as string;
        console.log(amount, key, value);
        size += key.length + value.length;
        amount++;
      }
      console.log('Total entries:', amount);
      console.log('Total size:', size);
      if (fShowMaximumSize === true) {
        const maxSize = LocalStorage.getMaximumSpace();
        console.log('Total size:', maxSize);
      }
    }

    public static load () : void {
        window['LocalStorage'] = LocalStorage
    }
}