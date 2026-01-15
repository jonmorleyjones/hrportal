import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { useTenantStore } from './tenantStore'
import { api } from '@/lib/api'
import * as utils from '@/lib/utils'
import type { Tenant } from '@/types'

// Mock the api module
vi.mock('@/lib/api', () => ({
  api: {
    setTenantSlug: vi.fn(),
    resolveTenant: vi.fn(),
  },
}))

// Mock extractSubdomain
vi.mock('@/lib/utils', async () => {
  const actual = await vi.importActual('@/lib/utils')
  return {
    ...actual,
    extractSubdomain: vi.fn(),
  }
})

describe('tenantStore', () => {
  const mockTenant: Tenant = {
    id: 'tenant-123',
    slug: 'acme',
    name: 'Acme Corporation',
    subscriptionTier: 'professional',
    settings: {
      enableNotifications: true,
      timezone: 'America/New_York',
      language: 'en',
    },
    branding: {
      logoUrl: null,
      primaryColor: '#007bff',
      secondaryColor: '#6c757d',
    },
  }

  beforeEach(() => {
    // Reset store state before each test
    useTenantStore.setState({
      tenant: null,
      isLoading: true,
      error: null,
    })
    vi.clearAllMocks()
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  describe('initial state', () => {
    it('should have null tenant initially', () => {
      const state = useTenantStore.getState()
      expect(state.tenant).toBeNull()
    })

    it('should be loading initially', () => {
      const state = useTenantStore.getState()
      expect(state.isLoading).toBe(true)
    })

    it('should have no error initially', () => {
      const state = useTenantStore.getState()
      expect(state.error).toBeNull()
    })
  })

  describe('resolveTenant', () => {
    it('should return false and set error when no subdomain exists', async () => {
      vi.mocked(utils.extractSubdomain).mockReturnValue(null)

      const { resolveTenant } = useTenantStore.getState()
      const result = await resolveTenant()

      expect(result).toBe(false)

      const state = useTenantStore.getState()
      expect(state.tenant).toBeNull()
      expect(state.isLoading).toBe(false)
      expect(state.error).toBe('No tenant specified. Please access via a tenant subdomain.')
    })

    it('should resolve tenant successfully', async () => {
      vi.mocked(utils.extractSubdomain).mockReturnValue('acme')
      vi.mocked(api.resolveTenant).mockResolvedValue(mockTenant)

      const { resolveTenant } = useTenantStore.getState()
      const result = await resolveTenant()

      expect(result).toBe(true)
      expect(api.setTenantSlug).toHaveBeenCalledWith('acme')
      expect(api.resolveTenant).toHaveBeenCalledWith('acme')

      const state = useTenantStore.getState()
      expect(state.tenant).toEqual(mockTenant)
      expect(state.isLoading).toBe(false)
      expect(state.error).toBeNull()
    })

    it('should set loading state while resolving', async () => {
      vi.mocked(utils.extractSubdomain).mockReturnValue('acme')

      let resolvePromise: ((value: Tenant) => void) | undefined
      vi.mocked(api.resolveTenant).mockReturnValue(
        new Promise((resolve) => {
          resolvePromise = resolve
        })
      )

      const { resolveTenant } = useTenantStore.getState()
      const promise = resolveTenant()

      // Should be loading
      expect(useTenantStore.getState().isLoading).toBe(true)
      expect(useTenantStore.getState().error).toBeNull()

      // Resolve the promise
      resolvePromise!(mockTenant)
      await promise

      // Should no longer be loading
      expect(useTenantStore.getState().isLoading).toBe(false)
    })

    it('should handle Error instance on failure', async () => {
      vi.mocked(utils.extractSubdomain).mockReturnValue('acme')
      vi.mocked(api.resolveTenant).mockRejectedValue(new Error('Tenant not found'))

      const { resolveTenant } = useTenantStore.getState()
      const result = await resolveTenant()

      expect(result).toBe(false)

      const state = useTenantStore.getState()
      expect(state.tenant).toBeNull()
      expect(state.isLoading).toBe(false)
      expect(state.error).toBe('Tenant not found')
    })

    it('should handle non-Error failure', async () => {
      vi.mocked(utils.extractSubdomain).mockReturnValue('acme')
      vi.mocked(api.resolveTenant).mockRejectedValue('Some string error')

      const { resolveTenant } = useTenantStore.getState()
      const result = await resolveTenant()

      expect(result).toBe(false)

      const state = useTenantStore.getState()
      expect(state.error).toBe('Failed to resolve tenant')
    })
  })

  describe('clearTenant', () => {
    it('should clear tenant state', () => {
      // Set some tenant state first
      useTenantStore.setState({
        tenant: mockTenant,
        isLoading: false,
        error: null,
      })

      const { clearTenant } = useTenantStore.getState()
      clearTenant()

      const state = useTenantStore.getState()
      expect(state.tenant).toBeNull()
      expect(state.isLoading).toBe(false)
      expect(state.error).toBeNull()
    })

    it('should call api.setTenantSlug with null', () => {
      const { clearTenant } = useTenantStore.getState()
      clearTenant()

      expect(api.setTenantSlug).toHaveBeenCalledWith(null)
    })

    it('should clear error state', () => {
      useTenantStore.setState({
        tenant: null,
        isLoading: false,
        error: 'Previous error',
      })

      const { clearTenant } = useTenantStore.getState()
      clearTenant()

      expect(useTenantStore.getState().error).toBeNull()
    })
  })
})
