import { UserManager, WebStorageStateStore } from "oidc-client-ts";

const apiUrl = import.meta.env.VITE_API_URL ?? "https://localhost:7182";

export const userManager = new UserManager({
  authority: import.meta.env.VITE_OIDC_AUTHORITY ?? "https://localhost:7182/",
  client_id: import.meta.env.VITE_OIDC_CLIENT_ID ?? "school-management-frontend",
  redirect_uri: `${window.location.origin}/auth/callback`,
  post_logout_redirect_uri: window.location.origin,
  response_type: "code",
  scope: "openid profile email roles api offline_access",
  loadUserInfo: true,
  userStore: new WebStorageStateStore({ store: window.sessionStorage }),
  automaticSilentRenew: true
});

export async function getAccessToken() {
  const user = await userManager.getUser();
  if (!user || user.expired) return null;
  return user.access_token;
}

export { apiUrl };
