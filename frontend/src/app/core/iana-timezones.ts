/** Fallback when `Intl.supportedValuesOf('timeZone')` is unavailable. */
const FALLBACK_IANA_TIME_ZONES = [
  'UTC',
  'America/New_York',
  'America/Chicago',
  'America/Denver',
  'America/Los_Angeles',
  'America/Phoenix',
  'America/Anchorage',
  'America/Lima',
  'America/Bogota',
  'America/Mexico_City',
  'America/Sao_Paulo',
  'America/Santiago',
  'America/Toronto',
  'America/Vancouver',
  'Europe/Madrid',
  'Europe/London',
  'Europe/Paris',
  'Europe/Berlin',
  'Africa/Cairo',
  'Asia/Tokyo',
  'Asia/Singapore',
  'Asia/Dubai',
  'Australia/Sydney',
  'Pacific/Auckland'
];

/** Sorted IANA ids for time-zone pickers (browser list when supported). */
export function listIanaTimeZones(): string[] {
  try {
    const intl = Intl as typeof Intl & { supportedValuesOf?: (key: string) => string[] };
    if (typeof intl.supportedValuesOf === 'function') {
      return intl.supportedValuesOf('timeZone').slice().sort((a, b) => a.localeCompare(b));
    }
  } catch {
    /* ignore */
  }
  return FALLBACK_IANA_TIME_ZONES.slice();
}
