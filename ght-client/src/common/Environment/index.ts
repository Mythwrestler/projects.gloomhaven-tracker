interface AuthEnvVars {
  Enabled: () => boolean;
  Domain: () => string;
  ClientId: () => string;
}

interface ApiEnvVars {
  BaseURL: () => string;
}

interface ClientEnvVars {
  BaseURL: () => string;
}

interface ServiceContext {
  Actions: string;
  State: string;
}

interface HubContext extends ServiceContext {
  Listeners: string;
  HubActions: string;
  HubState: string;
}

interface ContextEnvVars {
  ContentService: ServiceContext;
  CampaignService: ServiceContext;
  CombatService: ServiceContext;
  ActiveCombatHubService: HubContext;
  ActiveCombat: string;
  CombatHub: string;
}

const AUTH: AuthEnvVars = {
  Enabled: () => {
    return ("ENV_AUTH_ENABLED" ?? "false").toLowerCase() == "true";
  },
  Domain: () => {
    return "ENV_AUTH_DOMAIN";
  },
  ClientId: () => {
    return "ENV_AUTH_CLIENT_ID";
  },
};

const API: ApiEnvVars = {
  BaseURL: () => "ENV_API_BASE_URL",
};

const CLIENT: ClientEnvVars = {
  BaseURL: () => "ENV_CLIENT_BASE_URL",
};

const CONTEXT: ContextEnvVars = {
  ContentService: {
    Actions: "0fe59005-768d-4e9c-850f-54179004e63a",
    State: "6ef3b50a-6143-46c5-8f7f-33b621c1efad",
  },
  CampaignService: {
    Actions: "da85000a-cb93-4175-803b-69e0e53dd08b",
    State: "68ec1e97-5f6a-42d5-950a-78cf30bd810b",
  },
  CombatService: {
    Actions: "2de364fa-3081-45b1-8727-cfd8ee635179",
    State: "31a21366-9bdd-4d5d-9f6f-a1b6bebf01b4",
  },
  ActiveCombatHubService: {
    Actions: "1c7e5197-1c9c-498d-9610-824a4afee46e",
    State: "9fdc08a2-d57d-44a3-93d0-eec617c29fcd",
    Listeners: "b3f0fb93-ceec-4941-afa3-8356338ec017",
    HubActions: "0a368bbd-565b-41fe-b219-89681c4b874f",
    HubState: "baeca2da-96b7-4b88-9f12-b18669f4d552",
  },
  ActiveCombat: "41d944b7-4bab-492b-bb89-71a73bfe8b8d",
  CombatHub: "a67532f7-3434-4652-86d6-585a9ede30d6",
};

const ENV_VARS = {
  AUTH,
  API,
  CLIENT,
  CONTEXT,
};

export default ENV_VARS;
