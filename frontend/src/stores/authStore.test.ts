import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { useAuthStore } from './authStore'
import { api } from '@/lib/api'
import type { User } from '@/types'

// Mock the api module
vi.mock('@/lib/api', () => ({
  api: {
    setAccessToken: vi.fn(),
    refresh: vi.fn(),
  },
}))

describe('authStore', () => {
  const mockUser: User = {
    id: '123',
    email: 'test@example.com',
    name: 'Test User',
    role: 'Member',
    lastLoginAt: null,
    isActive: true,
  }

  beforeEach(() => {
    // Reset store state before each test
    useAuthStore.setState({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
    })
    vi.clearAllMocks()
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  describe('initial state', () => {
    it('should have null user initially', () => {
      const state = useAuthStore.getState()
      expect(state.user).toBeNull()
    })

    it('should have null tokens initially', () => {
      const state = useAuthStore.getState()
      expect(state.accessToken).toBeNull()
      expect(state.refreshToken).toBeNull()
    })

    it('should not be authenticated initially', () => {
      const state = useAuthStore.getState()
      expect(state.isAuthenticated).toBe(false)
    })
  })

  describe('setAuth', () => {
    it('should set user and tokens', () => {
      const { setAuth } = useAuthStore.getState()

      setAuth(mockUser, 'access-token', 'refresh-token')

      const state = useAuthStore.getState()
      expect(state.user).toEqual(mockUser)
      expect(state.accessToken).toBe('access-token')
      expect(state.refreshToken).toBe('refresh-token')
    })

    it('should set isAuthenticated to true', () => {
      const { setAuth } = useAuthStore.getState()

      setAuth(mockUser, 'access-token', 'refresh-token')

      expect(useAuthStore.getState().isAuthenticated).toBe(true)
    })

    it('should call api.setAccessToken', () => {
      const { setAuth } = useAuthStore.getState()

      setAuth(mockUser, 'access-token', 'refresh-token')

      expect(api.setAccessToken).toHaveBeenCalledWith('access-token')
    })
  })

  describe('clearAuth', () => {
    it('should clear all auth state', () => {
      const { setAuth, clearAuth } = useAuthStore.getState()

      setAuth(mockUser, 'access-token', 'refresh-token')
      clearAuth()

      const state = useAuthStore.getState()
      expect(state.user).toBeNull()
      expect(state.accessToken).toBeNull()
      expect(state.refreshToken).toBeNull()
      expect(state.isAuthenticated).toBe(false)
    })

    it('should call api.setAccessToken with null', () => {
      const { clearAuth } = useAuthStore.getState()

      clearAuth()

      expect(api.setAccessToken).toHaveBeenCalledWith(null)
    })
  })

  describe('refreshAuth', () => {
    it('should return false when no refresh token exists', async () => {
      const { refreshAuth } = useAuthStore.getState()

      const result = await refreshAuth()

      expect(result).toBe(false)
      expect(api.refresh).not.toHaveBeenCalled()
    })

    it('should refresh tokens successfully', async () => {
      vi.mocked(api.refresh).mockResolvedValue({
        accessToken: 'new-access-token',
        refreshToken: 'new-refresh-token',
      })

      // Set initial auth state
      useAuthStore.setState({
        user: mockUser,
        accessToken: 'old-access-token',
        refreshToken: 'old-refresh-token',
        isAuthenticated: true,
      })

      const { refreshAuth } = useAuthStore.getState()
      const result = await refreshAuth()

      expect(result).toBe(true)
      expect(api.refresh).toHaveBeenCalledWith('old-refresh-token')

      const state = useAuthStore.getState()
      expect(state.accessToken).toBe('new-access-token')
      expect(state.refreshToken).toBe('new-refresh-token')
    })

    it('should call api.setAccessToken with new token', async () => {
      vi.mocked(api.refresh).mockResolvedValue({
        accessToken: 'new-access-token',
        refreshToken: 'new-refresh-token',
      })

      useAuthStore.setState({
        refreshToken: 'old-refresh-token',
        isAuthenticated: true,
      })

      const { refreshAuth } = useAuthStore.getState()
      await refreshAuth()

      expect(api.setAccessToken).toHaveBeenCalledWith('new-access-token')
    })

    it('should clear auth and return false on refresh failure', async () => {
      vi.mocked(api.refresh).mockRejectedValue(new Error('Token expired'))

      useAuthStore.setState({
        user: mockUser,
        accessToken: 'old-access-token',
        refreshToken: 'old-refresh-token',
        isAuthenticated: true,
      })

      const { refreshAuth } = useAuthStore.getState()
      const result = await refreshAuth()

      expect(result).toBe(false)

      const state = useAuthStore.getState()
      expect(state.user).toBeNull()
      expect(state.accessToken).toBeNull()
      expect(state.refreshToken).toBeNull()
      expect(state.isAuthenticated).toBe(false)
    })
  })
})
