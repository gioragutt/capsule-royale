import { Schema } from '@colyseus/schema';

export type NoSchemaFields<T extends Schema> = Omit<T, keyof Schema>;
export type Update<T extends Schema> = Partial<NoSchemaFields<T>>;

export const randomId = ({ size = 6, alphabet = '0123456789ABCD' } = {}) => {
  let id = '';
  for (let i = 0; i < size; i++) {
    id += alphabet[Math.floor(Math.random() * alphabet.length)];
  }
  return id;
}

export function filterPermittedFields<T>(fields: Array<keyof T>, t: T): Partial<T> {
  return Object.fromEntries(Object.entries(t)
    .filter(([key, value]) => fields.includes(key as keyof T) && value !== undefined));
}