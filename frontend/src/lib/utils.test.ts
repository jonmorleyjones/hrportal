import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { cn, extractSubdomain, formatDate, formatDateTime, formatCurrency, formatBytes } from './utils'

describe('cn (className utility)', () => {
  it('should merge class names correctly', () => {
    expect(cn('foo', 'bar')).toBe('foo bar')
  })

  it('should handle conditional classes', () => {
    expect(cn('base', true && 'included', false && 'excluded')).toBe('base included')
  })

  it('should merge tailwind classes correctly', () => {
    expect(cn('px-2 py-1', 'px-4')).toBe('py-1 px-4')
  })

  it('should handle undefined and null values', () => {
    expect(cn('base', undefined, null, 'other')).toBe('base other')
  })

  it('should handle empty inputs', () => {
    expect(cn()).toBe('')
  })

  it('should handle array of classes', () => {
    expect(cn(['foo', 'bar'])).toBe('foo bar')
  })

  it('should handle object syntax', () => {
    expect(cn({ foo: true, bar: false, baz: true })).toBe('foo baz')
  })
})

describe('extractSubdomain', () => {
  const originalLocation = window.location

  beforeEach(() => {
    // @ts-expect-error - Mocking window.location
    delete window.location
  })

  afterEach(() => {
    window.location = originalLocation
  })

  it('should extract subdomain from localhost', () => {
    window.location = { hostname: 'acme.localhost' } as Location
    expect(extractSubdomain()).toBe('acme')
  })

  it('should return null for plain localhost', () => {
    window.location = { hostname: 'localhost' } as Location
    expect(extractSubdomain()).toBe(null)
  })

  it('should extract subdomain from production domain', () => {
    window.location = { hostname: 'acme.portal.com' } as Location
    expect(extractSubdomain()).toBe('acme')
  })

  it('should return null for two-part domain', () => {
    window.location = { hostname: 'portal.com' } as Location
    expect(extractSubdomain()).toBe(null)
  })

  it('should handle complex subdomains', () => {
    window.location = { hostname: 'tenant.app.example.com' } as Location
    expect(extractSubdomain()).toBe('tenant')
  })

  it('should handle nested localhost subdomains', () => {
    window.location = { hostname: 'tenant.dev.localhost' } as Location
    expect(extractSubdomain()).toBe('tenant')
  })
})

describe('formatDate', () => {
  it('should format date correctly', () => {
    const date = '2024-01-15T10:30:00Z'
    const result = formatDate(date)
    expect(result).toMatch(/Jan/)
    expect(result).toMatch(/15/)
    expect(result).toMatch(/2024/)
  })

  it('should handle different date formats', () => {
    const date = '2023-12-25'
    const result = formatDate(date)
    expect(result).toMatch(/Dec/)
    expect(result).toMatch(/25/)
    expect(result).toMatch(/2023/)
  })
})

describe('formatDateTime', () => {
  it('should format date and time correctly', () => {
    const dateTime = '2024-01-15T10:30:00Z'
    const result = formatDateTime(dateTime)
    expect(result).toMatch(/Jan/)
    expect(result).toMatch(/15/)
    expect(result).toMatch(/2024/)
  })

  it('should include time component', () => {
    const dateTime = '2024-06-20T14:45:00Z'
    const result = formatDateTime(dateTime)
    expect(result).toMatch(/\d{1,2}:\d{2}/)
  })
})

describe('formatCurrency', () => {
  it('should format positive amounts correctly', () => {
    expect(formatCurrency(1234.56)).toBe('$1,234.56')
  })

  it('should format zero correctly', () => {
    expect(formatCurrency(0)).toBe('$0.00')
  })

  it('should format negative amounts correctly', () => {
    expect(formatCurrency(-500)).toBe('-$500.00')
  })

  it('should format large amounts correctly', () => {
    expect(formatCurrency(1000000)).toBe('$1,000,000.00')
  })

  it('should round to two decimal places', () => {
    expect(formatCurrency(99.999)).toBe('$100.00')
  })
})

describe('formatBytes', () => {
  it('should format 0 bytes correctly', () => {
    expect(formatBytes(0)).toBe('0 Bytes')
  })

  it('should format bytes correctly', () => {
    expect(formatBytes(500)).toBe('500 Bytes')
  })

  it('should format kilobytes correctly', () => {
    expect(formatBytes(1024)).toBe('1 KB')
    expect(formatBytes(1536)).toBe('1.5 KB')
  })

  it('should format megabytes correctly', () => {
    expect(formatBytes(1048576)).toBe('1 MB')
    expect(formatBytes(2621440)).toBe('2.5 MB')
  })

  it('should format gigabytes correctly', () => {
    expect(formatBytes(1073741824)).toBe('1 GB')
  })

  it('should respect decimal precision', () => {
    expect(formatBytes(1536, 2)).toBe('1.5 KB')
    expect(formatBytes(1536, 0)).toBe('2 KB')
  })

  it('should handle negative decimals as 0', () => {
    expect(formatBytes(1536, -1)).toBe('2 KB')
  })
})
