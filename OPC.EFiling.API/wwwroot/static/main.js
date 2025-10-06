/*
 * Main JavaScript for the OPC e‑filing prototype.
 *
 * This script implements a simple, client‑side data store using
 * `window.localStorage` to persist users, instructions, drafts,
 * messages and timeline events.  All pages import this script
 * and call the exported functions to interact with the shared
 * state.  There is deliberately no backend server – the goal is
 * to provide an end‑to‑end demonstration that meets the terms of
 * reference using only HTML and JavaScript.  In production a
 * proper API and database would replace this layer.
 */

/**
 * Load or initialise persistent application data.  If
 * `localStorage.opcData` is undefined then a set of default
 * structures will be created.  This function should be called
 * on every page load before manipulating state.
 */
function loadData() {
  // Retrieve data from sessionStorage if possible.  Some browsers restrict
  // localStorage on file:// origins; sessionStorage is more permissive.
  let raw;
  try {
    raw = sessionStorage.getItem('opcData');
  } catch (e) {
    raw = null;
  }
  let data = raw ? JSON.parse(raw) : (window._opcData || null);
  if (!data) {
    // Initialise default data
    data = {
      users: [
        { id: 1, username: 'admin', password: 'admin', name: 'Administrator', role: 'admin', inbox: [], tasks: [] },
        { id: 2, username: 'drafter1', password: 'drafter1', name: 'Drafter One', role: 'drafter', inbox: [], tasks: [] },
        { id: 3, username: 'ministry1', password: 'ministry1', name: 'Ministry Clerk', role: 'ministry', inbox: [], tasks: [] },
        { id: 4, username: 'approver1', password: 'approver1', name: 'Approver', role: 'approver', inbox: [], tasks: [] }
      ],
      instructions: [
        {
          id: 1,
          title: 'Child Welfare Amendment Bill',
          description: 'Draft amendments to the Child Welfare Act to improve protection for vulnerable minors.',
          ministry: 3,
          assignedTo: 2,
          status: 'Drafting',
          drafts: [],
          createdAt: Date.now(),
          updatedAt: Date.now()
        }
      ],
      drafts: [],
      events: [],
      messages: [],
      templates: [
        { id: 1, name: 'Bill Template', description: 'Standard bill structure with pre‑defined headings and clauses.', content: '<h2>Title</h2>\n<p>An Act to ...</p>' },
        { id: 2, name: 'Regulation Template', description: 'Template for drafting regulations.', content: '<h2>Regulations</h2>\n<p>These regulations ...</p>' }
      ],
      currentUserId: null,
      currentInstructionId: null
    };
    // Create an initial draft for instruction 1
    const initialDraft = {
      id: 1,
      instructionId: 1,
      version: 1,
      content: '<h2>Child Welfare Amendment Bill</h2><p>This draft introduces amendments to improve child protection measures.</p>',
      timestamp: Date.now()
    };
    data.drafts.push(initialDraft);
    data.instructions[0].drafts.push(initialDraft.id);
    // Initial event
    data.events.push({ id: 1, instructionId: 1, type: 'DraftCreated', description: 'Draft created', timestamp: Date.now() });
    saveData(data);
  }
  return data;
}

/**
 * Persist application state to localStorage.  All modifications to
 * the data model should call this function after mutating the
 * returned object from `loadData()`.
 */
function saveData(data) {
  // Persist to sessionStorage when available, otherwise to a global
  try {
    sessionStorage.setItem('opcData', JSON.stringify(data));
  } catch (e) {
    window._opcData = data;
  }
}

/**
 * Authenticate a user by username and password.  If successful the
 * current user id is stored in the data model.  Returns true on
 * success and false otherwise.
 */
function login(username, password) {
  const data = loadData();
  const user = data.users.find(u => u.username === username && u.password === password);
  if (user) {
    data.currentUserId = user.id;
    saveData(data);
    return true;
  }
  return false;
}

/**
 * Log out the current user.  Clears the currentUserId and refreshes
 * the page.
 */
function logout() {
  const data = loadData();
  data.currentUserId = null;
  saveData(data);
  window.location.href = 'index.html';
}

/**
 * Retrieve the current logged in user object.  Returns null if not
 * logged in.
 */
function getCurrentUser() {
  const data = loadData();
  return data.users.find(u => u.id === data.currentUserId) || null;
}

/**
 * Update navbar badges for inbox and tasks.  Should be called on
 * each page load after the user has been authenticated.
 */
function updateBadges() {
  const data = loadData();
  const user = getCurrentUser();
  if (!user) return;
  const inboxCount = data.messages.filter(m => m.recipientId === user.id && !m.read).length;
  const tasksCount = data.instructions.filter(instr => instr.assignedTo === user.id).length;
  const inboxBadge = document.getElementById('inboxBadge');
  const tasksBadge = document.getElementById('tasksBadge');
  if (inboxBadge) inboxBadge.textContent = inboxCount;
  if (tasksBadge) tasksBadge.textContent = tasksCount;
}

/**
 * Create a new user.  Accessible only to admins.  Returns the new
 * user object.  Roles can be 'admin', 'drafter', 'ministry' or
 * 'approver'.
 */
function createUser(username, password, name, role) {
  const data = loadData();
  const id = data.users.reduce((max, u) => Math.max(max, u.id), 0) + 1;
  const user = { id, username, password, name, role, inbox: [], tasks: [] };
  data.users.push(user);
  saveData(data);
  return user;
}

/**
 * Add a new instruction.  Typically used by a ministry user to create
 * a new drafting request.  The assignedTo argument should be the id
 * of the drafter responsible for the draft.
 */
function createInstruction(title, description, ministryId, assignedTo) {
  const data = loadData();
  const id = data.instructions.reduce((max, ins) => Math.max(max, ins.id), 0) + 1;
  const instr = {
    id,
    title,
    description,
    ministry: ministryId,
    assignedTo,
    status: 'Drafting',
    drafts: [],
    createdAt: Date.now(),
    updatedAt: Date.now()
  };
  data.instructions.push(instr);
  // Add to drafter's tasks
  const drafter = data.users.find(u => u.id === assignedTo);
  if (drafter) drafter.tasks.push(id);
  saveData(data);
  return instr;
}

/**
 * Add a new draft to an instruction.  Increments the version
 * automatically.  The content should be HTML retrieved from a
 * WYSIWYG editor.  Creates a timeline event.
 */
function saveDraft(instructionId, content) {
  const data = loadData();
  const instruction = data.instructions.find(i => i.id === instructionId);
  if (!instruction) return;
  const nextVersion = instruction.drafts.length ? (data.drafts.find(d => d.id === instruction.drafts[instruction.drafts.length - 1]).version + 1) : 1;
  const id = data.drafts.reduce((max, d) => Math.max(max, d.id), 0) + 1;
  const draft = { id, instructionId, version: nextVersion, content, timestamp: Date.now() };
  data.drafts.push(draft);
  instruction.drafts.push(id);
  instruction.updatedAt = Date.now();
  // Create event
  const eventId = data.events.reduce((max, e) => Math.max(max, e.id), 0) + 1;
  data.events.push({ id: eventId, instructionId, type: 'DraftSaved', description: 'Draft version ' + nextVersion + ' saved', timestamp: Date.now() });
  saveData(data);
  return draft;
}

/**
 * Send a draft to the ministry.  Marks the instruction status,
 * generates an event and sends a message to the ministry user.
 */
function sendDraftToMinistry(instructionId) {
  const data = loadData();
  const instruction = data.instructions.find(i => i.id === instructionId);
  if (!instruction) return;
  instruction.status = 'Pending Ministry';
  instruction.updatedAt = Date.now();
  // Create event
  const eventId = data.events.reduce((max, e) => Math.max(max, e.id), 0) + 1;
  data.events.push({ id: eventId, instructionId, type: 'DraftSentToMinistry', description: 'Draft sent to ministry', timestamp: Date.now() });
  // Send message to ministry user
  const ministryUser = data.users.find(u => u.id === instruction.ministry);
  if (ministryUser) {
    const msgId = data.messages.reduce((max, m) => Math.max(max, m.id), 0) + 1;
    data.messages.push({ id: msgId, senderId: instruction.assignedTo, recipientId: ministryUser.id, subject: 'Draft ready: ' + instruction.title, content: 'Please review the latest draft of "' + instruction.title + '".', timestamp: Date.now(), read: false });
  }
  saveData(data);
}

/**
 * Ministry sends feedback to drafter.  Adds message and event and
 * optionally changes status back to drafting.
 */
function ministryFeedback(instructionId, messageContent) {
  const data = loadData();
  const instruction = data.instructions.find(i => i.id === instructionId);
  if (!instruction) return;
  instruction.status = 'Drafting';
  instruction.updatedAt = Date.now();
  // Event
  const eventId = data.events.reduce((max, e) => Math.max(max, e.id), 0) + 1;
  data.events.push({ id: eventId, instructionId, type: 'MinistryFeedback', description: 'Feedback received from ministry', timestamp: Date.now() });
  // Message to drafter
  const drafter = data.users.find(u => u.id === instruction.assignedTo);
  if (drafter) {
    const msgId = data.messages.reduce((max, m) => Math.max(max, m.id), 0) + 1;
    data.messages.push({ id: msgId, senderId: instruction.ministry, recipientId: drafter.id, subject: 'Feedback: ' + instruction.title, content: messageContent, timestamp: Date.now(), read: false });
  }
  saveData(data);
}

/**
 * Ministry approves draft and forwards to approver.  Updates status,
 * creates event and sends message to approver.
 */
function ministryApprove(instructionId, approverId) {
  const data = loadData();
  const instr = data.instructions.find(i => i.id === instructionId);
  if (!instr) return;
  instr.status = 'Pending Approval';
  instr.updatedAt = Date.now();
  // Event
  const eventId = data.events.reduce((max, e) => Math.max(max, e.id), 0) + 1;
  data.events.push({ id: eventId, instructionId, type: 'MinistryApproved', description: 'Ministry approved draft', timestamp: Date.now() });
  // Message to approver
  const msgId = data.messages.reduce((max, m) => Math.max(max, m.id), 0) + 1;
  data.messages.push({ id: msgId, senderId: instr.ministry, recipientId: approverId, subject: 'Approval needed: ' + instr.title, content: 'Please review and sign off the draft of "' + instr.title + '".', timestamp: Date.now(), read: false });
  saveData(data);
}

/**
 * Approver signs off a draft.  Marks status as Approved and publishes
 * the law (creates an event and message to printing office).  For
 * demonstration the printing office is assumed to be user with role
 * 'printing'.  If none exists the message is omitted.
 */
function signDraft(instructionId) {
  const data = loadData();
  const instr = data.instructions.find(i => i.id === instructionId);
  if (!instr) return;
  instr.status = 'Approved';
  instr.updatedAt = Date.now();
  // Event
  const eventId = data.events.reduce((max, e) => Math.max(max, e.id), 0) + 1;
  data.events.push({ id: eventId, instructionId, type: 'Approved', description: 'Draft approved and signed', timestamp: Date.now() });
  // Publish message to printing office (role printing)
  const printing = data.users.find(u => u.role === 'printing');
  if (printing) {
    const msgId = data.messages.reduce((max, m) => Math.max(max, m.id), 0) + 1;
    data.messages.push({ id: msgId, senderId: instr.assignedTo, recipientId: printing.id, subject: 'Publish law: ' + instr.title, content: 'Please publish the approved law.', timestamp: Date.now(), read: false });
  }
  saveData(data);
}

/**
 * Mark a message as read.
 */
function markMessageRead(messageId) {
  const data = loadData();
  const msg = data.messages.find(m => m.id === messageId);
  if (msg) {
    msg.read = true;
    saveData(data);
  }
}

/**
 * Change the name of the current user.
 */
function changeName(newName) {
  const data = loadData();
  const user = getCurrentUser();
  if (user) {
    user.name = newName;
    saveData(data);
  }
}

// Expose functions globally
window.OPC = {
  loadData,
  saveData,
  login,
  logout,
  getCurrentUser,
  updateBadges,
  createUser,
  createInstruction,
  saveDraft,
  sendDraftToMinistry,
  ministryFeedback,
  ministryApprove,
  signDraft,
  markMessageRead,
  changeName
};